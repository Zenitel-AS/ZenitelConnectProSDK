using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ConnectPro.Tools;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.Core.Services;

public sealed class ConnectProService : IConnectProService
{
    private readonly ILogger<ConnectProService> _log;
    private readonly IConfigStore _configStore;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private bool _started;
    private bool _disposed;

    public ConnectPro.Core Core { get; private set; } = null!;

    public ConnectProService(ILogger<ConnectProService> log, IConfigStore configStore)
    {
        _log = log;
        _configStore = configStore;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            if (_started) return;

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
            if (Core is not null)
            {
                Core.Events.OnExceptionThrown?.Invoke(this, ex);
            }

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
            Core.DeviceHandler.RetrieveRegisteredDevices();
        }
        finally
        {
            _gate.Release();
        }

    }

    private Task<ConnectPro.Configuration> LoadConfigurationAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var defaults = ConnectPro.Configuration.GetDefaultConfiguration();
        var saved = _configStore.LoadForMachine(Environment.MachineName);
        var cfg = saved ?? defaults;

        cfg.MachineName = Environment.MachineName;
        cfg.ServerAddr = string.IsNullOrWhiteSpace(cfg.ServerAddr) ? defaults.ServerAddr : cfg.ServerAddr;
        cfg.Port = string.IsNullOrWhiteSpace(cfg.Port) ? defaults.Port : cfg.Port;
        cfg.Realm = string.IsNullOrWhiteSpace(cfg.Realm) ? defaults.Realm : cfg.Realm;
        cfg.UserName ??= "";
        cfg.OperatorDirNo ??= "";
        cfg.Password = Cryptography.Decrypt(cfg.Password ?? "");

        return Task.FromResult(cfg);
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
