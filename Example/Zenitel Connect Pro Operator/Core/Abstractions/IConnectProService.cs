using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZenitelConnectProOperator.Core.Abstractions;

public interface IConnectProService : IAsyncDisposable
{
    ConnectPro.Core Core { get; }

    Task StartAsync(CancellationToken ct);
    Task StopAsync(CancellationToken ct);
    Task RefreshAsync(CancellationToken ct);

}
