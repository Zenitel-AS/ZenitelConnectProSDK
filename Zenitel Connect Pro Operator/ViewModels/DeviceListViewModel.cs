using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectPro.Models;
using ConnectPro.Models.AccessControl;
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
        _connectPro.Core.Events.OnDoorOpen += HandleDoorOpenEvent;
        _connectPro.Core.Events.OnGpioEvent += HandleGPIOEvent;

        QueueRefresh();
    }

    private void HandleDoorOpenEvent(object? sender, OpenDoorEventData eventData)
    {
        var time = eventData.EventTime;
        var lastEventTime = eventData.LastEventTimestamp;
        var doorDIrno = eventData.DoorDirno;
        var fromDirno = eventData.FromDirno;
        var isPostSuccess = eventData.IsSuccess; // This one is used only when POST-ing the event to ZCP
        var eventInformation = eventData.EventInformationMessage;
    }

    private void HandleGPIOEvent(object sender, WampGpioEventArgs e)
    {
        var dirno = e.Dirno;
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
            _connectPro.Core.Events.OnDoorOpen -= HandleDoorOpenEvent;
        }
    }
}
