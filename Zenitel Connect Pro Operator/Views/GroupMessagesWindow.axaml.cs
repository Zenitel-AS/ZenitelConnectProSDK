using Avalonia.Controls;
using ConnectPro.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using ZenitelConnectProOperator.Core.Abstractions;
using ZenitelConnectProOperator.ViewModels;

namespace ZenitelConnectProOperator.Views;

public partial class GroupMessagesWindow : Window
{
    public ICommand? TogglePlayCommand => (DataContext as GroupMessagesViewModel)?.TogglePlayCommand;

    public GroupMessagesWindow()
    {
        InitializeComponent();
    }

    public GroupMessagesWindow(Group group, GroupsViewModel ownerVm)
    {
        InitializeComponent();
        var connectPro = App.Services.GetRequiredService<IConnectProService>();
        var vm = new GroupMessagesViewModel(group, connectPro);
        vm.RequestClose += (_, _) => Close();
        DataContext = vm;
    }
}
