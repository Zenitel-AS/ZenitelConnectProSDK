using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ZenitelConnectProOperator.Core.Abstractions;
using ZenitelConnectProOperator.Views;

namespace ZenitelConnectProOperator.Core.Services;

public sealed class WindowService : IWindowService
{
    private Window? _configurationWindow;
    private Window? _toneTestWindow;

    public void ShowOrActivateConfiguration()
        => ShowOrActivate(
            get: () => _configurationWindow,
            set: v => _configurationWindow = v,
            factory: () => new ConfigurationWindow());

    public void ShowOrActivateToneTest()
        => ShowOrActivate(
            get: () => _toneTestWindow,
            set: v => _toneTestWindow = v,
            factory: () => new ToneTestWindow());

    private static void ShowOrActivate(Func<Window?> get, Action<Window?> set, Func<Window> factory)
    {
        var existing = get();
        if (existing is { IsVisible: true })
        {
            existing.Activate();
            return;
        }

        var w = factory();
        set(w);

        w.Closed += (_, _) =>
        {
            // Guard in case something else replaced the slot before close
            if (ReferenceEquals(get(), w))
                set(null);
        };

        var owner = GetMainWindow();
        if (owner is not null)
        {
            w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            w.Show(owner);
        }
        else
        {
            w.Show();
        }
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;

        return null;
    }
}
