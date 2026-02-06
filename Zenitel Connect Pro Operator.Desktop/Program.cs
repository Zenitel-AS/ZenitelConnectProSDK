using System;
using Avalonia;
using ZenitelConnectProOperator; // <-- REQUIRED
using ZenitelConnectProOperator.Desktop.Platform;

namespace ZenitelConnectProOperator.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // ✅ Platform-specific registration (CORRECT LOCATION)
        PlatformRegistration.DatabasePathProviderFactory =
            () => new DesktopDatabasePathProvider();

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
