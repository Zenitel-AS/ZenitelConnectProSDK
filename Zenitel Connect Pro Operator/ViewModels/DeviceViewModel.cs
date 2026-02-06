using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ConnectPro.Enums;
using ConnectPro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ZenitelConnectProOperator.ViewModels;

public partial class DeviceViewModel : ObservableObject, IDisposable
{
    public Device Device { get; }

    [ObservableProperty] private bool _gpiosLoaded;
    [ObservableProperty] private int _gpiCount;
    [ObservableProperty] private int _gpoCount;
    [ObservableProperty] private int _activeGpiCount;
    [ObservableProperty] private int _activeGpoCount;

    // Collapsed by default (false)
    [ObservableProperty] private bool _isGpioExpanded;

    public ObservableCollection<GpioPointViewModel> Gpios { get; } = new();

    public int GpioCount => GpiCount + GpoCount;
    public bool HasGpios => GpioCount > 0;
    public int ActiveGpioTotalCount => _activeGpiCount + _activeGpoCount;
    public bool HasActiveGpio => ActiveGpioTotalCount > 0;

    private bool _wired;

    public DeviceViewModel(Device device)
    {
        Device = device ?? throw new ArgumentNullException(nameof(device));
        WireGpio();
    }

    private void WireGpio()
    {
        if (_wired) return;

        var gpio = Device.Gpio;
        if (gpio is null) return;

        _wired = true;

        // Snapshot upsert + live events
        gpio.Changed += OnGpioChanged;

        // Kick off initial snapshot sync
        _ = LoadInitialSnapshotAsync();
    }

    private async Task LoadInitialSnapshotAsync()
    {
        var gpio = Device.Gpio;
        if (gpio is null) return;

        try
        {
            await gpio.WhenInitializedAsync().ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                RefreshPointsFromDevice();
                RefreshCountsFromDevice();
                GpiosLoaded = true;
            });
        }
        catch
        {
            // optional: log
        }
    }

    private void OnGpioChanged(object? sender, GpioChangedEventArgs e)
    {
        _ = Dispatcher.UIThread.InvokeAsync(() =>
        {
            GpiosLoaded = true;
            UpsertPoint(e.Point);
            RefreshCountsFromDevice();
        });
    }

    private void RefreshPointsFromDevice()
    {
        var gpio = Device.Gpio;
        if (gpio is null) return;

        var points = (gpio.Inputs ?? Array.Empty<GpioPoint>())
            .Concat(gpio.Outputs ?? Array.Empty<GpioPoint>())
            .OrderBy(p => p.Direction)
            .ThenBy(p => p.Id)
            .ToList();

        Gpios.Clear();
        foreach (var p in points)
            Gpios.Add(new GpioPointViewModel(p));
    }

    private void UpsertPoint(GpioPoint point)
    {
        // Key by (Direction, Id)
        var existingIndex = -1;
        for (int i = 0; i < Gpios.Count; i++)
        {
            if (Gpios[i].Id == point.Id && Gpios[i].Direction == point.Direction)
            {
                existingIndex = i;
                break;
            }
        }

        if (existingIndex < 0)
        {
            // Insert in sorted order for stable UI
            var insertIndex = 0;
            while (insertIndex < Gpios.Count)
            {
                var cur = Gpios[insertIndex];
                if (point.Direction < cur.Direction) break;
                if (point.Direction == cur.Direction && string.CompareOrdinal(point.Id, cur.Id) < 0) break;
                insertIndex++;
            }

            Gpios.Insert(insertIndex, new GpioPointViewModel(point));
        }
        else
        {
            // Replace the item in the collection so the ItemsControl
            // sees a CollectionChanged (Replace) and re-renders the visual.
            Gpios[existingIndex] = new GpioPointViewModel(point);
        }
    }

    private void RefreshCountsFromDevice()
    {
        var gpio = Device.Gpio;
        if (gpio is null) return;

        GpiCount = gpio.Inputs?.Count ?? 0;
        GpoCount = gpio.Outputs?.Count ?? 0;
        ActiveGpiCount = gpio.Inputs?.Count(p => p.State == GpioState.Active) ?? 0;
        ActiveGpoCount = gpio.Outputs?.Count(p => p.State == GpioState.Active) ?? 0;

        OnPropertyChanged(nameof(GpioCount));
        OnPropertyChanged(nameof(HasGpios));
        OnPropertyChanged(nameof(ActiveGpioTotalCount));
        OnPropertyChanged(nameof(HasActiveGpio));
    }

    public void Dispose()
    {
        var gpio = Device.Gpio;
        if (gpio is not null && _wired)
            gpio.Changed -= OnGpioChanged;

        _wired = false;
    }

    /// <summary>
    /// Called from the DeviceListViewModel when a GPIO event arrives through
    /// the legacy Events path. Updates the UI-bound view models directly.
    /// Must be called on the UI thread.
    /// </summary>
    public void UpdateGpioFromEvent(GpioPoint point)
    {
        GpiosLoaded = true;
        UpsertPoint(point);
        RefreshCountsFromDevice();
    }
}

public sealed partial class GpioPointViewModel : ObservableObject
{
    [ObservableProperty] private string _id;
    [ObservableProperty] private GpioDirection _direction;
    [ObservableProperty] private GpioState _state;
    [ObservableProperty] private DateTimeOffset _updatedUtc;
    [ObservableProperty] private string _rawState = string.Empty;

    public GpioPointViewModel(GpioPoint point) => Update(point);

    public void Update(GpioPoint point)
    {
        Id = point.Id;
        Direction = point.Direction;
        State = point.State;
        UpdatedUtc = point.UpdatedUtc;
        RawState = point.RawState ?? string.Empty;
    }
}
