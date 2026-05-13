using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Input;
using ZenitelConnectProOperator.ViewModels;

namespace ZenitelConnectProOperator.Views;

public partial class GroupsView : UserControl
{
    public ICommand? InitiateGroupCallCommand => (DataContext as GroupsViewModel)?.InitiateGroupCallCommand;
    public ICommand? OpenMessagesCommand => (DataContext as GroupsViewModel)?.OpenMessagesCommand;

    public GroupsView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new GroupsViewModel();
            return;
        }

        DataContext = ActivatorUtilities.CreateInstance<GroupsViewModel>(App.Services);


        if (DataContext is GroupsViewModel vm)
        {
            vm.RequestOpenGroupCallPopup += (_, group) =>
            {
                var w = new GroupCallPopupWindow(group, vm);
                w.Show();
                w.Activate();
            };

            vm.RequestOpenGroupMessages += (_, group) =>
            {
                var w = new GroupMessagesWindow(group, vm);
                w.Show();
                w.Activate();
            };
        }
    }
}
