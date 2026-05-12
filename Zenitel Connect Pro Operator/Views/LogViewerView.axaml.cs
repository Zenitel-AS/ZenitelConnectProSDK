using Avalonia.Controls;
using Avalonia.Interactivity;
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

    private async void CopyLog_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not LogViewerViewModel viewModel)
            return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
            return;

        await clipboard.SetTextAsync(viewModel.GetLogText());
    }
}
