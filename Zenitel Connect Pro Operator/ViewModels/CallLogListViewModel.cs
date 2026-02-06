using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectPro.Models;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.ViewModels;

public partial class CallLogListViewModel : ObservableObject, IDisposable
{
    private readonly IConnectProService? _connectPro;
    private readonly ILogStore? _logStore;

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentQueue<CallLog> _queue = new();
    private volatile bool _isProcessing;
    private DateTime _lastUiRefresh = DateTime.MinValue;

    // Match WPF intent: throttle + recent duplicate window. :contentReference[oaicite:1]{index=1}
    private static readonly TimeSpan UiThrottle = TimeSpan.FromMilliseconds(1000);
    private static readonly TimeSpan DuplicateWindow = TimeSpan.FromSeconds(5);
    private const int MaxRows = 100;

    public ObservableCollection<CallLog> Log { get; } = new();

    [ObservableProperty] private string statusText = "Ready";

    // Design-time ctor
    public CallLogListViewModel()
    {
        Log.Add(new CallLog
        {
            Time = DateTime.Now,
            DeviceName = "Entrance A",
            FromDirno = "1001",
            ToDirno = "9999",
            AnsweredByDirno = "9999",
            State = "connected"
        });
    }

    public CallLogListViewModel(IConnectProService connectPro, ILogStore logStore)
    {
        _connectPro = connectPro;
        _logStore = logStore;

        // Initial load (WPF does this in ctor from CoreHandler.LogContext). :contentReference[oaicite:2]{index=2}
        _ = RefreshAsync();

        // Subscribe to event source (Avalonia-side service, not UI code-behind).
        _connectPro.Core.Events.OnCallLogEntryAdded += OnCallLogEntryAdded;
    }

    private void OnCallLogEntryAdded(object? sender, CallLog e)
    {
        // WPF checks duplicates already in queue before enqueueing. :contentReference[oaicite:3]{index=3}
        var existsInQueue = _queue.Any(log =>
            log.DeviceName == e.DeviceName &&
            log.FromDirno == e.FromDirno &&
            log.ToDirno == e.ToDirno &&
            log.AnsweredByDirno == e.AnsweredByDirno &&
            log.State == e.State);

        if (!existsInQueue)
            _queue.Enqueue(e);

        _ = ProcessQueueAsync();
    }

    [RelayCommand]
    private Task Refresh() => RefreshAsync();

    private async Task RefreshAsync()
    {
        if (_logStore is null) return;

        try
        {
            StatusText = "Loading…";
            var latest = await _logStore.GetLatestCallLogsAsync(MaxRows);

            Log.Clear();
            foreach (var row in latest.OrderByDescending(x => x.Time))
                Log.Add(row);

            StatusText = $"Loaded {Log.Count}";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    private async Task ProcessQueueAsync()
    {
        if (_isProcessing) return;

        _isProcessing = true;
        await _semaphore.WaitAsync();
        try
        {
            while (_queue.TryDequeue(out var entry))
            {
                // throttle refresh + write frequency (WPF: 1s). :contentReference[oaicite:4]{index=4}
                if (_lastUiRefresh != DateTime.MinValue &&
                    (DateTime.Now - _lastUiRefresh) < UiThrottle)
                {
                    continue;
                }

                _lastUiRefresh = DateTime.Now;

                if (_logStore is null)
                    continue;

                StatusText = "Writing…";

                // Recent-duplicate prevention window (WPF: last 5s). :contentReference[oaicite:5]{index=5}
                var windowStart = DateTime.Now.Subtract(DuplicateWindow);

                var exists = await _logStore.ExistsSimilarAsync(entry, windowStart);
                if (!exists)
                    await _logStore.AddAsync(entry);

                var latest = await _logStore.GetLatestCallLogsAsync(MaxRows);

                Log.Clear();
                foreach (var row in latest.OrderByDescending(x => x.Time))
                    Log.Add(row);

                StatusText = $"Updated {Log.Count}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            _semaphore.Release();
            _isProcessing = false;
        }
    }

    public void Dispose()
    {
        if (_connectPro is not null)
            _connectPro.Core.Events.OnCallLogEntryAdded -= OnCallLogEntryAdded;

        _semaphore.Dispose();
    }
}
