using Avalonia.Controls;
using ConnectPro.Models;
using ZenitelConnectProOperator.ViewModels;

namespace ZenitelConnectProOperator.Views;

public partial class GroupCallPopupWindow : Window
{
    public GroupCallPopupWindow()
    {
        InitializeComponent();
    }

    public GroupCallPopupWindow(Group group, GroupsViewModel ownerVm)
    {
        InitializeComponent();

        var vm = new GroupCallPopupViewModel(group, ownerVm);
        vm.RequestClose += (_, _) => Close();
        DataContext = vm;
    }
}
