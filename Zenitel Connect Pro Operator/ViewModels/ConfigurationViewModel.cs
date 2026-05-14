using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectPro.Models;
using ConnectPro.Tools;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.ViewModels;

public partial class ConfigurationViewModel : ObservableObject
{
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
    [ObservableProperty] private string connectedSystemText = "-";
    [ObservableProperty] private string connectionStatusText = "Disconnected";
    [ObservableProperty] private IBrush connectionStatusBrush = Brushes.Red;

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
        _connectPro.Core.Events.OnDeviceRetrievalStart += OnDeviceRetrievalStart;
        _connectPro.Core.Events.OnDeviceRetrievalEnd += OnDeviceRetrievalEnd;
        _connectPro.Core.Events.OnExceptionThrown += OnExceptionThrown;

        ApplyInitialState();
    }

    partial void OnIsBusyChanged(bool value)
        => OnPropertyChanged(nameof(CanConnect));

    private void ApplyInitialState()
    {
        var handler = _connectPro.Core.ConnectionHandler;

        if (handler?.IsConnected == true)
        {
            SetStatus("Connected", Brushes.LightGray, false, _connectPro.Core.Configuration.ServerAddr);
            RefreshOperatorList();
            return;
        }

        if (handler?.IsReconnecting == true)
        {
            SetStatus("Connecting...", Brushes.Gold, true, _connectPro.Core.Configuration.ServerAddr);
            return;
        }

        SetStatus("Disconnected", Brushes.Red, false, "-");
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
            ConnectionStatusBrush = brush;
            IsBusy = busy;

            if (connectedSystem is not null)
            {
                ConnectedSystemText = string.IsNullOrWhiteSpace(connectedSystem) ? "-" : connectedSystem;
            }
        });
    }

    private void OnConnectionChanged(object? sender, bool isConnected)
    {
        if (isConnected)
        {
            SetStatus(
                "Connected - synchronizing devices...",
                Brushes.Gold,
                true,
                _connectPro.Core.Configuration.ServerAddr);
        }
        else
        {
            SetStatus("Disconnected", Brushes.Red, false, "-");
        }
    }

    private void OnDeviceRetrievalStart(object? sender, EventArgs e)
    {
        SetStatus(
            "Connecting - retrieving devices...",
            Brushes.Gold,
            true,
            _connectPro.Core.Configuration.ServerAddr);
    }

    private void OnDeviceRetrievalEnd(object? sender, EventArgs e)
    {
        RefreshOperatorList();

        if (_connectPro.Core.ConnectionHandler?.IsConnected == true)
        {
            SetStatus(
                "Connected",
                Brushes.LightGray,
                false,
                _connectPro.Core.Configuration.ServerAddr);
        }
    }

    private void OnExceptionThrown(object? sender, Exception ex)
    {
        SetStatus($"Error: {ex.Message}", Brushes.OrangeRed, false, "-");
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
            SetStatus($"Connecting to {ServerAddr}:{Port}...", Brushes.Gold, true, ServerAddr);

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
            SetStatus($"Error: {ex.Message}", Brushes.OrangeRed, false, "-");
        }
    }

    public sealed record OperatorOption(string Display, string? DirNo);
}
