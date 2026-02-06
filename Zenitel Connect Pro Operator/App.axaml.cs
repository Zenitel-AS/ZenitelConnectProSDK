using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZenitelConnectProOperator.Views;
using ZenitelConnectProOperator.Core.Abstractions;
using ZenitelConnectProOperator.Core.Services;
using ZenitelConnectProOperator.Data.Db;
using ZenitelConnectProOperator.ViewModels;
using System;

namespace ZenitelConnectProOperator;

public sealed class App : Application
{
    public static IHost Host { get; private set; } = null!;
    public static IServiceProvider Services => Host.Services;


    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override async void OnFrameworkInitializationCompleted()
    {
        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<IDatabasePathProvider>(_ =>
                {
                    if (PlatformRegistration.DatabasePathProviderFactory is null)
                        throw new System.InvalidOperationException("IDatabasePathProvider not registered by platform project.");

                    return PlatformRegistration.DatabasePathProviderFactory();
                });

                // Register all shared services (stores, handlers, etc.)
                PlatformRegistration.Register(services);

                services.AddSingleton<IConnectProService, ConnectProService>();

                services.AddDbContextFactory<OperationDbContext>((sp, opt) =>
                {
                    var paths = sp.GetRequiredService<IDatabasePathProvider>();
                    var dbPath = paths.GetDatabasePath("zenitel-operator.db");
                    opt.UseSqlite($"Data Source={dbPath}");
                });

                services.AddDbContextFactory<LogDbContext>((sp, opt) =>
                {
                    var paths = sp.GetRequiredService<IDatabasePathProvider>();
                    var dbPath = paths.GetDatabasePath("zenitel-operator-log.db");
                    opt.UseSqlite($"Data Source={dbPath}");
                    #if DEBUG
                        opt.EnableSensitiveDataLogging();
                    #endif
                });

                services.AddSingleton<MainViewModel>();
            })
            .Build();

        try
        {
            // Ensure DB created + migrated
            try
            {
                var dbFactory = Host.Services.GetRequiredService<IDbContextFactory<OperationDbContext>>();
                await using (var db = await dbFactory.CreateDbContextAsync())
                {
                    await db.Database.EnsureCreatedAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OperationDbContext init failed: {ex}");
                throw;
            }

            try
            {
                var logFactory = Host.Services.GetRequiredService<IDbContextFactory<LogDbContext>>();
                await using (var logDb = await logFactory.CreateDbContextAsync())
                {
                    await logDb.Database.EnsureCreatedAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogDbContext init failed: {ex}");
                throw;
            }

            // Start ConnectProService
            try
            {
                var zcp = Host.Services.GetRequiredService<IConnectProService>();
                await zcp.StartAsync(default);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConnectProService startup failed: {ex}");
                throw;
            }

            // Force DeviceSyncService instantiation so it subscribes to events
            Host.Services.GetRequiredService<IDeviceSyncService>();

            // UI setup
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Host.Services.GetRequiredService<MainViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Framework initialization failed: {ex}");
            throw;
        }
    }
}
