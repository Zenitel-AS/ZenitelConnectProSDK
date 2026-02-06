using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.ViewModels;

public partial class LogViewerViewModel : ObservableObject, IDisposable
{
    private readonly IConnectProService? _connectPro;
    private bool _disposed;

    private const int MaxLogEntries = 500;

    public ObservableCollection<LogEntryViewModel> LogEntries { get; } = new();

    // Previewer / fallback ctor
    public LogViewerViewModel()
    {
        LogEntries.Add(new LogEntryViewModel(DateTime.Now.AddMinutes(-2), "Sample log entry 1"));
        LogEntries.Add(new LogEntryViewModel(DateTime.Now.AddMinutes(-1), "Sample log entry 2"));
        LogEntries.Add(new LogEntryViewModel(DateTime.Now, "Sample log entry 3"));
    }

    // Real ctor (DI)
    public LogViewerViewModel(IConnectProService connectPro)
    {
        _connectPro = connectPro ?? throw new ArgumentNullException(nameof(connectPro));

        _connectPro.Core.Events.OnChildLogEntry += HandleLogEntry;
        _connectPro.Core.Events.OnExceptionThrown += HandleException;
        _connectPro.Core.Events.OnDebugChanged += HandleDebugChanged;
    }

    private void HandleException(object? sender, Exception ex)
    {
        if (_disposed || ex is null)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            if (_disposed)
                return;

            LogEntries.Insert(0, new LogEntryViewModel(DateTime.Now, $"[ERROR] {ex.Message}"));

            while (LogEntries.Count > MaxLogEntries)
                LogEntries.RemoveAt(LogEntries.Count - 1);
        });
    }

    private void HandleDebugChanged(object? sender, (string, object) debugInfo)
    {
        if (_disposed || string.IsNullOrEmpty(debugInfo.Item1))
            return;

        Dispatcher.UIThread.Post(() =>
        {
            if (_disposed)
                return;

            var message = debugInfo.Item2 != null 
                ? $"[DEBUG] {debugInfo.Item1}: {debugInfo.Item2}" 
                : $"[DEBUG] {debugInfo.Item1}";
            LogEntries.Insert(0, new LogEntryViewModel(DateTime.Now, message));

            while (LogEntries.Count > MaxLogEntries)
                LogEntries.RemoveAt(LogEntries.Count - 1);
        });
    }

    private void HandleLogEntry(object? sender, string message)
    {
        if (_disposed || string.IsNullOrEmpty(message))
            return;

        Dispatcher.UIThread.Post(() =>
        {
            if (_disposed)
                return;

            LogEntries.Insert(0, new LogEntryViewModel(DateTime.Now, message));

            // Trim old entries to prevent unbounded growth
            while (LogEntries.Count > MaxLogEntries)
                LogEntries.RemoveAt(LogEntries.Count - 1);
        });
    }

    [RelayCommand]
    private void Clear()
    {
        LogEntries.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_connectPro is not null)
        {
            _connectPro.Core.Events.OnChildLogEntry -= HandleLogEntry;
            _connectPro.Core.Events.OnExceptionThrown -= HandleException;
            _connectPro.Core.Events.OnDebugChanged -= HandleDebugChanged;
        }
    }
}

public partial class LogEntryViewModel : ObservableObject
{
    public DateTime Timestamp { get; }
    public string Message { get; }
    public string FormattedTimestamp => Timestamp.ToString("HH:mm:ss.fff");

    public LogEntryViewModel(DateTime timestamp, string message)
    {
        Timestamp = timestamp;
        Message = message;
    }
}
