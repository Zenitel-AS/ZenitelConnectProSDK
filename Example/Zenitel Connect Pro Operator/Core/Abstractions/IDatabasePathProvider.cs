namespace ZenitelConnectProOperator.Core.Abstractions;

public interface IDatabasePathProvider
{
    string GetDatabasePath(string fileName);
}
