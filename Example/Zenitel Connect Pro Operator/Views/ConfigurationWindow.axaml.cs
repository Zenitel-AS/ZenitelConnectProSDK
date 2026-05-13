using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using ZenitelConnectProOperator.ViewModels;

namespace ZenitelConnectProOperator.Views;

public partial class ConfigurationWindow : Window
{
    public ConfigurationWindow()
    {
        InitializeComponent();

        // Resolve ConfigurationViewModel from the DI container
        var vm = App.Host.Services.GetRequiredService<ConfigurationViewModel>();
        DataContext = vm;

        // Let the VM close the window without knowing about Avalonia APIs.
        vm.RequestClose += (_, _) => Close();
    }
}
