using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Input;
using ZenitelConnectProOperator.Core.Services;
using ZenitelConnectProOperator.ViewModels;

namespace ZenitelConnectProOperator.Views;

public partial class DeviceListView : UserControl
{
    public ICommand? ToggleCallCommand => (DataContext as DeviceListViewModel)?.ToggleCallCommand;

    public DeviceListView()
    {
        InitializeComponent();

        // Prefer DI if available, but keep previewer-friendly fallback.
        if (Design.IsDesignMode)
        {
            DataContext = new DeviceListViewModel();
            return;
        }

        // If you have a global ServiceProvider pattern, use it.
        // If not, you can remove this and set DataContext from parent view.
        DataContext ??= App.Current?
            .GetType()
            .GetProperty("Services")?
            .GetValue(App.Current) is IServiceProvider sp
                ? ActivatorUtilities.CreateInstance<DeviceListViewModel>(sp)
                : new DeviceListViewModel();
    }
}
