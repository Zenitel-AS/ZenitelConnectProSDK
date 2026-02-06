namespace ZenitelConnectProOperator.Core.Abstractions;

public interface IDeviceSyncService
{
    void TriggerSync(ConnectPro.Core core);
}
