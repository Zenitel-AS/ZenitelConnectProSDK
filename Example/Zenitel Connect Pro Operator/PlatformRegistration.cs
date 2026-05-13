using Microsoft.Extensions.DependencyInjection;
using System;
using ZenitelConnectProOperator.Core.Abstractions;
using ZenitelConnectProOperator.Core.Services;
using ZenitelConnectProOperator.Data.Db;
using ZenitelConnectProOperator.ViewModels;

namespace ZenitelConnectProOperator;

public static class PlatformRegistration
{
    public static Func<IDatabasePathProvider>? DatabasePathProviderFactory { get; set; }

    public static void Register(IServiceCollection services)
    {
        // Stores
        services.AddSingleton<Core.Abstractions.IConfigStore, EfConfigStore>();
        services.AddSingleton<ILogStore, EfLogStore>();

        // Core services
        services.AddSingleton<IDeviceSyncService, DeviceSyncService>();

        // Replace Facade with Service
        services.AddSingleton<IConnectProService, ConnectProService>();

        // Application services
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<IAudioService, AudioService>();

        // ViewModels
        services.AddSingleton<ConfigurationViewModel>();

        //Transient is usually best for viewmodels per-view. Singleton would make the VM “global” and can keep stale subscriptions.
        services.AddTransient<GroupsViewModel>();


    }
}
