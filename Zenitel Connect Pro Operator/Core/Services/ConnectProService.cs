using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.Core.Services;

public sealed class ConnectProService : IConnectProService
{
    private readonly ILogger<ConnectProService> _log;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private bool _started;
    private bool _disposed;

    public ConnectPro.Core Core { get; private set; } = null!;

    public ConnectProService(ILogger<ConnectProService> log)
    {
        _log = log;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            if (_started) return;

            // TODO: Replace with config repository (SQLite) + secure password handling.
            var cfg = await LoadConfigurationAsync(ct);

            Core = new ConnectPro.Core
            {
                Configuration = cfg
            };

            Core.Start();
            _started = true;

            _log.LogInformation("ConnectProService started (Server={Server}:{Port})", cfg.ServerAddr, cfg.Port);
        }
        catch (Exception ex)
        {
            Core.Events.OnExceptionThrown?.Invoke(this, ex);
            throw;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task StopAsync(CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            if (!_started) return;

            Core.Dispose();

            _started = false;
            _log.LogInformation("ConnectProService stopped.");
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RefreshAsync(CancellationToken ct)
    {
               await _gate.WaitAsync(ct);
        try
        {
            if (!_started) throw new InvalidOperationException("Service not started.");
            await Core.DeviceHandler.RetrieveRegisteredDevices();
        }
        finally
        {
            _gate.Release();
        }

    }

    private static Task<ConnectPro.Configuration> LoadConfigurationAsync(CancellationToken ct)
    {
        // Placeholder defaults (same as your original)
        return Task.FromResult(new ConnectPro.Configuration
        {
            ServerAddr = "192.168.1.5",
            Port = "8086",
            UserName = "",
            Password = "",
            MachineName = Environment.MachineName,
            Realm = ""
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        try { await StopAsync(CancellationToken.None); }
        catch { /* ignore dispose errors */ }

        _gate.Dispose();
    }
}
