using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectPro.Enums;
using ConnectPro.Models;
using ConnectPro.Models.GPIO;
using Wamp.Client;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.ViewModels;

public partial class DeviceListViewModel : ObservableObject, IDisposable
{
    private readonly IConnectProService? _connectPro;

    public ObservableCollection<DeviceViewModel> Devices { get; } = new();

    // Prevent refresh re-entrancy (events can burst).
    private int _refreshQueued;
    private bool _disposed;

    // Previewer / fallback ctor
    public DeviceListViewModel()
    {
        Devices.Add(new DeviceViewModel(new Device { dirno = "1001", name = "Entrance A" }));
        Devices.Add(new DeviceViewModel(new Device { dirno = "1002", name = "Gate West" }));
    }

    // Real ctor (DI)
    public DeviceListViewModel(IConnectProService connectPro)
    {
        _connectPro = connectPro ?? throw new ArgumentNullException(nameof(connectPro));

        _connectPro.Core.Events.OnDeviceListChange += OnDevicesChanged;
        _connectPro.Core.Events.OnOperatorDirNoChange += OnOperatorChanged;
        _connectPro.Core.Events.OnGpioEvent += HandleGpioEvent;

        QueueRefresh();
    }

    private void OnDevicesChanged(object? sender, EventArgs e) => QueueRefresh();

    private void OnOperatorChanged(object? sender, string e) => QueueRefresh();

    private void QueueRefresh()
    {
        if (_disposed || _connectPro is null)
            return;

        // collapse bursts into a single UI-thread refresh
        if (Interlocked.Exchange(ref _refreshQueued, 1) == 1)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (_disposed || _connectPro is null)
                    return;

                RefreshDeviceListOnUiThread();
            }
            finally
            {
                Interlocked.Exchange(ref _refreshQueued, 0);
            }
        });
    }

    private void RefreshDeviceListOnUiThread()
    {
        foreach (var vm in Devices)
            vm.Dispose();

        Devices.Clear();

        var registered = _connectPro!.Core.Collection.RegisteredDevices;
        if (registered is null || registered.Count == 0)
            return;

        var snapshot = registered.ToList();

        foreach (var device in snapshot)
        {
            if (device is null)
                continue;

            Devices.Add(new DeviceViewModel(device));
        }
    }

    private void HandleGpioEvent(object? sender, WampGpioEventArgs wgea)
    {
        _connectPro.Core.Events.OnChildLogEntry.Invoke(this, 
            $"Received GPIO event for device {wgea.Dirno}, element {wgea.Element.id}, state {wgea.Element.state}, operation {wgea.Element.operation}");

        // Determine the direction and state from the event
        Device? device = _connectPro?.Core.Collection.RegisteredDevices.FirstOrDefault(d => d.dirno == wgea.Dirno);
        if (device?.Gpio is null)
            return;

        GpioDirection direction;
        if (device.Gpio.Inputs.Any(i => i.Id == wgea.Element.id))
            direction = GpioDirection.Gpi;
        else if (device.Gpio.Outputs.Any(o => o.Id == wgea.Element.id))
            direction = GpioDirection.Gpo;
        else
        {
            _connectPro?.Core.Events.OnChildLogEntry.Invoke(this,
                $"GPIO Point {wgea.Element.id} not found for device {device.dirno}");
            return;
        }

        var updatedPoint = new GpioPoint(
            wgea.Element.id,
            direction,
            ParseGpioState(wgea.Element),
            DateTimeOffset.UtcNow,
            wgea.Element.state ?? wgea.Element.operation ?? string.Empty);

        // Find the matching DeviceViewModel and push the update through the UI layer
        Dispatcher.UIThread.Post(() =>
        {
            var vm = Devices.FirstOrDefault(d => d.Device.dirno == wgea.Dirno);
            vm?.UpdateGpioFromEvent(updatedPoint);
        });
    }

    private static GpioState ParseGpioState(WampClient.wamp_device_gpio_element element)
    {
        // Primary: use "state" field when present ("low"/"high")
        if (!string.IsNullOrEmpty(element.state))
        {
            if (element.state.Equals("high", StringComparison.OrdinalIgnoreCase))
                return GpioState.Active;

            if (element.state.Equals("low", StringComparison.OrdinalIgnoreCase))
                return GpioState.Inactive;

            if (element.state == "1") return GpioState.Active;
            if (element.state == "0") return GpioState.Inactive;
        }

        // Fallback: GPO events use "operation" ("set"/"clear") instead of "state"
        if (!string.IsNullOrEmpty(element.operation))
        {
            if (element.operation.Equals("set", StringComparison.OrdinalIgnoreCase))
                return GpioState.Active;

            if (element.operation.Equals("clear", StringComparison.OrdinalIgnoreCase))
                return GpioState.Inactive;
        }

        return GpioState.Unknown;
    }
    private static async Task SafeRefreshGpio(Device device)
    {
        try
        {
            // Gpio is runtime-attached and may be null depending on how the Device was created.
            // Device.Gpio is NOT mapped/persisted and is attached by the SDK at runtime. :contentReference[oaicite:1]{index=1}
            if (device.Gpio is null)
                return;

            await device.Gpio.RefreshAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            // Swallow: UI list rendering must not fail due to transient GPIO/WAMP issues.
            // If you want diagnostics, inject a logger later.
        }
    }

    [RelayCommand]
    private async Task ToggleCall(Device device)
    {
        if (_connectPro is null || device is null)
            return;

        if (device.CallState != ConnectPro.Enums.CallState.reachable)
        {
            await _connectPro.Core.CallHandler.DeleteCall(device.dirno);
        }
        else
        {
            var op = _connectPro.Core.Configuration.OperatorDirNo;
            if (string.IsNullOrWhiteSpace(op))
                return;

            await _connectPro.Core.CallHandler.PostCall(op, device.dirno, "setup");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_connectPro is not null)
        {
            _connectPro.Core.Events.OnDeviceListChange -= OnDevicesChanged;
            _connectPro.Core.Events.OnOperatorDirNoChange -= OnOperatorChanged;
            _connectPro.Core.Events.OnGpioEvent -= HandleGpioEvent;
        }
    }
}
