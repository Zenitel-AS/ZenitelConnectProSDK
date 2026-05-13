using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using ZenitelConnectProOperator.ViewModels;

namespace ZenitelConnectProOperator.Views;

public partial class CallForwardingView : UserControl
{
    public ICommand? ToggleEnabledCommand => (DataContext as CallForwardingViewModel)?.ToggleEnabledCommand;
    public ICommand? DeleteRuleCommand => (DataContext as CallForwardingViewModel)?.DeleteRuleCommand;

    public CallForwardingView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new CallForwardingViewModel();
            return;
        }

        DataContext = ActivatorUtilities.CreateInstance<CallForwardingViewModel>(App.Services);
    }
}
