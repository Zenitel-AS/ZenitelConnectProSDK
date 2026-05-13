using ConnectPro; // if your Configuration type is ConnectPro.Configuration

namespace ZenitelConnectProOperator.Core.Abstractions;

public interface IConfigStore
{
    void Initialize();
    Configuration? LoadForMachine(string machineName);
    void SaveConfiguration(Configuration configuration);
}
