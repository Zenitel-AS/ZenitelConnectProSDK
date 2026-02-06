using System;
using System.IO;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.Desktop.Platform;

public sealed class DesktopDatabasePathProvider : IDatabasePathProvider
{
    public string GetDatabasePath(string fileName)
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(root, "ZenitelConnectProOperator");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, fileName);
    }
}
