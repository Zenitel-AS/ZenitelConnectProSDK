using System;
using System.Threading;
using ConnectPro;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.Core.Services;

public sealed class DeviceSyncService : IDeviceSyncService
{
    private readonly IConfigStore _configStore; // or a dedicated IDeviceStore later
    private int _syncInProgress;

    public DeviceSyncService(IConfigStore configStore)
    {
        _configStore = configStore;
    }

    public void TriggerSync(ConnectPro.Core core)
    {
        // Prevent re-entrancy storms from OnDeviceListChange / OnDeviceRetrievalEnd
        if (Interlocked.Exchange(ref _syncInProgress, 1) == 1) return;

        try
        {
            // TODO: implement real upsert against your Data layer.
            // Keep this empty until your EF/data model is in place.
        }
        catch (Exception)
        {
            // swallow or forward to log store depending on your preference
        }
        finally
        {
            Interlocked.Exchange(ref _syncInProgress, 0);
        }
    }
}
