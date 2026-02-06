using Avalonia.Controls;
using ConnectPro.Models;
using System.Windows.Input;
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
        var vm = new GroupMessagesViewModel(group);
        vm.RequestClose += (_, _) => Close();
        DataContext = vm;
    }
}
