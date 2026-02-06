using System;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IConnectProService _connectPro;
    private readonly IWindowService _windows;
    private readonly IAudioService _audio;

    [ObservableProperty] private string operatorText = "Operator: -";
    [ObservableProperty] private string connectionStatusText = "Not Connected!";
    [ObservableProperty] private IBrush connectionStatusBrush = Brushes.Red;
    [ObservableProperty] private int selectedTabIndex;

    public MainViewModel(IConnectProService connectPro, IWindowService windows, IAudioService audio)
    {
        _connectPro = connectPro;
        _windows = windows;
        _audio = audio;

        _connectPro.Core.Events.OnOperatorDirNoChange += OnOperatorDirNoChanged;
        _connectPro.Core.Events.OnConnectionChanged += OnConnectionChanged;

        // Init DB + load config + start SDK + maybe reconnect (your existing CoreRuntime behavior)
        

        ApplyInitialState();
    }

    private void ApplyInitialState()
    {
        OperatorText = $"Operator: {_connectPro.Core.Configuration.OperatorDirNo ?? "-"}";

        if (_connectPro.Core.ConnectionHandler.IsConnected)
        {
            ConnectionStatusText = $"Connected to {_connectPro.Core.Configuration.ServerAddr}";
            ConnectionStatusBrush = Brushes.LightGray;
        }
        else
        {
            ConnectionStatusText = "Not Connected!";
            ConnectionStatusBrush = Brushes.Red;
        }
    }

    private void OnOperatorDirNoChanged(object? sender, string operatorDirNo)
    {
        Dispatcher.UIThread.Post(() =>
        {
            OperatorText = $"Operator: {_connectPro.Core.Configuration.OperatorDirNo ?? operatorDirNo}";
        });
    }

    private void OnConnectionChanged(object? sender, bool isConnected)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (isConnected)
            {
                ConnectionStatusText = $"Connected to {_connectPro.Core.Configuration.ServerAddr}";
                ConnectionStatusBrush = Brushes.LightGray;
                _audio.PlayConnected();
            }
            else
            {
                ConnectionStatusText = "Not Connected!";
                ConnectionStatusBrush = Brushes.Red;
            }
        });
    }

    [RelayCommand] private void OpenConfig() => _windows.ShowOrActivateConfiguration();
    [RelayCommand] private void OpenToneTest() => _windows.ShowOrActivateToneTest();

    [RelayCommand]
    private Task RefreshAsync() => _connectPro.RefreshAsync(System.Threading.CancellationToken.None);

    public void Dispose()
    {
        _connectPro.Core.Events.OnOperatorDirNoChange -= OnOperatorDirNoChanged;
        _connectPro.Core.Events.OnConnectionChanged -= OnConnectionChanged;

        // Optional: if you want VM lifetime to own runtime lifetime:
        // _runtime.Dispose();
    }
}
