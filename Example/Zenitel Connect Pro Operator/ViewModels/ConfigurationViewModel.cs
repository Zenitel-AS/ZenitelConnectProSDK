using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectPro;
using ConnectPro.Models;
using ConnectPro.Tools;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.ViewModels;

public partial class ConfigurationViewModel : ObservableObject
{
    private enum UiConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        ConnectedSynchronizing,
        Synced,
        Error
    }
    public sealed record OperatorOption(string Display, string? DirNo);

    private readonly IConnectProService _connectPro;
    private readonly IConfigStore _configStore;

    private bool _suppressOperatorPersist;
    private string? _loadedOperatorDirNo;

    public event EventHandler? RequestClose;

    [ObservableProperty] private string serverAddr = "10.8.33.5";
    [ObservableProperty] private string port = "8086";
    [ObservableProperty] private string realm = "";
    [ObservableProperty] private string userName = "";
    [ObservableProperty] private string password = "";

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string connectionStatusText = "Disconnected";

    private string _connectedSystemDisplay = "-";
    public string ConnectedSystemDisplay
    {
        get => _connectedSystemDisplay;
        set => SetProperty(ref _connectedSystemDisplay, value);
    }

    private IBrush _connectionStatusDisplayBrush = Brushes.Red;
    public IBrush ConnectionStatusDisplayBrush
    {
        get => _connectionStatusDisplayBrush;
        set => SetProperty(ref _connectionStatusDisplayBrush, value);
    }

    public ObservableCollection<OperatorOption> OperatorOptions { get; } =
        new()
        {
            new OperatorOption("-", null),
        };

    [ObservableProperty] private OperatorOption? selectedOperator;

    public bool CanConnect => !IsBusy;

    public ConfigurationViewModel(IConnectProService connectPro, IConfigStore configStore)
    {
        _connectPro = connectPro;
        _configStore = configStore;

        SelectedOperator = OperatorOptions.FirstOrDefault();

        // Load saved configuration on init
        LoadSavedConfiguration();

        // Wire service events to update UI
        _connectPro.Core.Events.OnConnectionChanged += OnConnectionChanged;
        _connectPro.Core.Events.OnOperatorDirNoChange += OnOperatorDirNoChanged;
        _connectPro.Core.Events.OnDeviceRetrievalEnd += OnDeviceRetrievalEnd;
        _connectPro.Core.Events.OnExceptionThrown += OnExceptionThrown;

        ApplyInitialState();
    }

    partial void OnIsBusyChanged(bool value)
        => OnPropertyChanged(nameof(CanConnect));

    private void ApplyInitialState()
    {
        var handler = _connectPro.Core.ConnectionHandler;
        var serverAddr = _connectPro.Core.Configuration.ServerAddr;

        if (handler?.IsConnected == true)
        {
            ApplyUiStatus(UiConnectionState.Connected, serverAddr);
            RefreshOperatorList();
            return;
        }

        if (handler?.IsReconnecting == true)
        {
            ApplyUiStatus(UiConnectionState.Connecting, serverAddr);
            return;
        }

        ApplyUiStatus(UiConnectionState.Disconnected);
    }

    private void LoadSavedConfiguration()
    {
        try
        {
            var saved = _configStore.LoadForMachine(Environment.MachineName);
            if (saved is null)
                return;

            ServerAddr = saved.ServerAddr ?? "10.8.33.5";
            Port = saved.Port ?? "8086";
            Realm = saved.Realm ?? "";
            UserName = saved.UserName ?? "";
            Password = Cryptography.Decrypt(saved.Password ?? "");

            // Keep this and apply it when we have options to match against.
            _loadedOperatorDirNo = saved.OperatorDirNo;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load configuration: {ex.Message}");
        }
    }

    private void SaveCurrentConfiguration()
    {
        var config = new ConnectPro.Configuration
        {
            MachineName = Environment.MachineName,
            ServerAddr = ServerAddr,
            Port = Port,
            Realm = Realm,
            UserName = UserName,
            Password = Cryptography.Encrypt(Password),
            OperatorDirNo = SelectedOperator?.DirNo
        };

        _configStore.SaveConfiguration(config);
    }

    private void SetStatus(string status, IBrush brush, bool busy, string? connectedSystem = null)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ConnectionStatusText = status;
            ConnectionStatusDisplayBrush = brush;
            IsBusy = busy;

            if (connectedSystem is not null)
            {
                ConnectedSystemDisplay = string.IsNullOrWhiteSpace(connectedSystem) ? "-" : connectedSystem;
            }
        });
    }

    private void OnConnectionChanged(object? sender, bool isConnected)
    {
        if (isConnected)
        {
            if (!_connectPro.Core.Collection.RegisteredDevices.Any())
                ApplyUiStatus(UiConnectionState.ConnectedSynchronizing, _connectPro.Core.Configuration.ServerAddr);

            else
                ApplyUiStatus(UiConnectionState.Connected, _connectPro.Core.Configuration.ServerAddr);
        }
        else
        {
            ApplyUiStatus(UiConnectionState.Disconnected);
        }
    }

    private void OnDeviceRetrievalEnd(object? sender, EventArgs e)
    {
        RefreshOperatorList();

        if (_connectPro.Core.ConnectionHandler?.IsConnected == true)
        {
            ApplyUiStatus(
                UiConnectionState.Synced,
                _connectPro.Core.Configuration.ServerAddr);
        }
    }

    private void OnExceptionThrown(object? sender, Exception ex)
    {
        ApplyUiStatus(UiConnectionState.Error, errorMessage: ex.Message);
    }

    private void OnOperatorDirNoChanged(object? sender, string dirNo)
    {
        if (string.IsNullOrWhiteSpace(dirNo))
            return;

        var option = OperatorOptions.FirstOrDefault(o => o.DirNo == dirNo);
        if (option is null)
            return;

        _suppressOperatorPersist = true;
        try
        {
            SelectedOperator = option;
        }
        finally
        {
            _suppressOperatorPersist = false;
        }
    }

    private void RefreshOperatorList()
    {
        try
        {
            _suppressOperatorPersist = true;
            try
            {
                OperatorOptions.Clear();
                OperatorOptions.Add(new OperatorOption("-", null));

                var devices = _connectPro.Core.Collection.RegisteredDevices;
                if (devices is not null && devices.Count > 0)
                {
                    foreach (Device device in devices)
                    {
                        OperatorOptions.Add(new OperatorOption($"{device.name} [{device.dirno}]", device.dirno));
                    }
                }

                // Choose operator in this order:
                // 1) SDK current OperatorDirNo
                // 2) saved operator from config
                // 3) "-"
                var preferDirNo =
                    _connectPro.Core.Configuration.OperatorDirNo
                    ?? _loadedOperatorDirNo;

                if (!string.IsNullOrWhiteSpace(preferDirNo))
                {
                    var match = OperatorOptions.FirstOrDefault(o => o.DirNo == preferDirNo);
                    if (match is not null)
                        SelectedOperator = match;
                }

                SelectedOperator ??= OperatorOptions.FirstOrDefault();
            }
            finally
            {
                _suppressOperatorPersist = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to refresh operator list: {ex.Message}");
        }
    }

    // ✅ Persist immediately when dropdown changes (ComboBox selection)
    partial void OnSelectedOperatorChanged(OperatorOption? value)
    {
        if (_suppressOperatorPersist)
            return;

        try
        {
            // Keep SDK in sync right away (optional but useful)
            _connectPro.Core.Configuration.OperatorDirNo = value?.DirNo;

            // Persist immediately
            SaveCurrentConfiguration();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to persist operator selection: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Close()
        => RequestClose?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private async Task Connect()
    {
        try
        {
            ApplyUiStatus(UiConnectionState.Connecting, ServerAddr);

            // Persist current config (includes selected operator)
            SaveCurrentConfiguration();

            // Apply configuration to SDK
            var config = new ConnectPro.Configuration
            {
                MachineName = Environment.MachineName,
                ServerAddr = ServerAddr,
                Port = Port,
                Realm = Realm,
                UserName = UserName,
                Password = Password,
                OperatorDirNo = SelectedOperator?.DirNo
            };

            _connectPro.Core.Configuration = config;
            _connectPro.Core.Events.OnConfigurationChanged?.Invoke(this, config);

            // Reconnect SDK with new configuration
            await _connectPro.Core.ConnectionHandler.RecoonectAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Connection failed: {ex.Message}");
            ApplyUiStatus(UiConnectionState.Error, errorMessage: ex.Message);
        }
    }


    private void ApplyUiStatus(
    UiConnectionState state,
    string? connectedSystem = null,
    string? errorMessage = null)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (state)
            {
                case UiConnectionState.Disconnected:
                    ConnectionStatusText = "Disconnected";
                    ConnectionStatusDisplayBrush = Brushes.Red;
                    IsBusy = false;
                    ConnectedSystemDisplay = "-";
                    break;

                case UiConnectionState.Connecting:
                    ConnectionStatusText = $"Connecting to {ServerAddr}:{Port}...";
                    ConnectionStatusDisplayBrush = Brushes.Gold;
                    IsBusy = true;
                    ConnectedSystemDisplay = NormalizeConnectedSystem(connectedSystem);
                    break;

                case UiConnectionState.Connected:
                    ConnectionStatusText = "Connected - Not synced";
                    ConnectionStatusDisplayBrush = Brushes.Gold;
                    IsBusy = true;
                    ConnectedSystemDisplay = NormalizeConnectedSystem(connectedSystem);
                    break;

                case UiConnectionState.ConnectedSynchronizing:
                    ConnectionStatusText = "Connecting - retrieving devices...";
                    ConnectionStatusDisplayBrush = Brushes.Gold;
                    IsBusy = true;
                    ConnectedSystemDisplay = NormalizeConnectedSystem(connectedSystem);
                    break;

                case UiConnectionState.Synced:
                    ConnectionStatusText = "Connected";
                    ConnectionStatusDisplayBrush = Brushes.LightGray;
                    IsBusy = false;
                    ConnectedSystemDisplay = NormalizeConnectedSystem(connectedSystem);
                    break;

                case UiConnectionState.Error:
                    ConnectionStatusText = string.IsNullOrWhiteSpace(errorMessage)
                        ? "Error"
                        : $"Error: {errorMessage}";

                    ConnectionStatusDisplayBrush = Brushes.OrangeRed;
                    IsBusy = false;
                    ConnectedSystemDisplay = "-";
                    break;

                default:
                    ConnectionStatusText = "Unknown";
                    ConnectionStatusDisplayBrush = Brushes.OrangeRed;
                    IsBusy = false;
                    ConnectedSystemDisplay = "-";
                    break;
            }
        });
    }

    private static string NormalizeConnectedSystem(string? connectedSystem)
    {
        return string.IsNullOrWhiteSpace(connectedSystem)
            ? "-"
            : connectedSystem;
    }
}
