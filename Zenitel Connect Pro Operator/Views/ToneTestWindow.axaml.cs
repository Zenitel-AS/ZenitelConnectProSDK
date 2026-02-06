using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using ZenitelConnectProOperator.ViewModels;

namespace ZenitelConnectProOperator.Views;

public partial class ToneTestWindow : Window
{
    public ToneTestWindow()
    {
        InitializeComponent();

        DataContext ??= new ToneTestWindowViewModel();

        if (DataContext is ToneTestWindowViewModel vm)
        {
            vm.RequestClose += (_, _) => Close();

            // Keep the header checkbox in sync when VM updates SelectAll
            vm.PropertyChanged += VmOnPropertyChanged;
        }
    }

    private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ToneTestWindowViewModel.SelectAll))
        {
            // Re-notify Avalonia binding system that our proxy changed
            RaisePropertyChanged(SelectAllProperty, default, SelectAll);
        }
    }

    public static readonly DirectProperty<ToneTestWindow, bool?> SelectAllProperty =
        AvaloniaProperty.RegisterDirect<ToneTestWindow, bool?>(
            nameof(SelectAll),
            o => o.SelectAll,
            (o, v) => o.SelectAll = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public bool? SelectAll
    {
        get => (DataContext as ToneTestWindowViewModel)?.SelectAll;
        set
        {
            if (DataContext is ToneTestWindowViewModel vm)
                vm.SelectAll = value;
        }
    }
}
