using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using ZenitelConnectProOperator.ViewModels;

namespace ZenitelConnectProOperator.Views;

public partial class CallLogListView : UserControl
{
    public CallLogListView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new CallLogListViewModel();
            return;
        }

        DataContext ??= App.Current?
            .GetType()
            .GetProperty("Services")?
            .GetValue(App.Current) is IServiceProvider sp
                ? ActivatorUtilities.CreateInstance<CallLogListViewModel>(sp)
                : new CallLogListViewModel();
    }
}
