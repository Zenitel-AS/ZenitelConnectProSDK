using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using ZenitelConnectProOperator.ViewModels;

namespace ZenitelConnectProOperator.Views;

public partial class LogViewerView : UserControl
{
    public LogViewerView()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new LogViewerViewModel();
            return;
        }

        DataContext ??= App.Current?
            .GetType()
            .GetProperty("Services")?
            .GetValue(App.Current) is IServiceProvider sp
                ? ActivatorUtilities.CreateInstance<LogViewerViewModel>(sp)
                : new LogViewerViewModel();
    }
}
