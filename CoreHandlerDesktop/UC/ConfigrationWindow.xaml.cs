using CoreHandlerDesktop;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ZenitelConnectMIP.UIElements
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window, INotifyPropertyChanged
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
            DataContext = this;
            PasswordBox.Password = CoreHandler.Core.Configuration.Password;

            // Subscribe to connection change event
            CoreHandler.Core.Events.OnDeviceRetrievalEnd += HandleDeviceListRetrieved;

            // Initialize device list
           // DeviceList = new ObservableCollection<ConnectPro.Models.Device>();
            DirectoryNumbers = new ObservableCollection<string>();

            // Populate the combobox if already connected
            RefreshDeviceList();
        }

        public ConnectPro.Configuration Configuration
        {
            get { return CoreHandler.Core.Configuration; }
            set
            {
                CoreHandler.Core.Configuration = value;
                CoreHandler.Core.Events.OnConfigurationChanged?.Invoke(this, CoreHandler.Core.Configuration);
            }
        }

        private ObservableCollection<string> _directoryNumbers;
        public ObservableCollection<string> DirectoryNumbers
        {
            get => _directoryNumbers;
            set
            {
                _directoryNumbers = value;
                OnPropertyChanged(nameof(DirectoryNumbers)); // ✅ Notify WPF
            }
        }

        private void HandleDeviceListRetrieved(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RefreshDeviceList();
            });
        }

        private void RefreshDeviceList()
        {
            if (CoreHandler.Core.ConnectionHandler.IsConnected)
            {
                // Fetch available devices when connected
                DirectoryNumbers.Clear();
                foreach (var device in CoreHandler.Core.Collection.RegisteredDevices)
                {
                    //DeviceList.Add(device);
                    DirectoryNumbers.Add(device.dirno);
                }
            }
            else
            {
                // Clear device list when disconnected
                DirectoryNumbers.Clear();
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                Configuration.Password = passwordBox.Password;
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            CoreHandler.Core.Configuration.ServerAddr = txtIpAddress.Text;
            CoreHandler.Core.Configuration.Port       = txtPort.Text;
            CoreHandler.Core.Configuration.Realm      = txtRealm.Text;
            CoreHandler.Core.Configuration.UserName   = txtUsername.Text;
            CoreHandler.Core.Configuration.Password   = PasswordBox.Password;
            CoreHandler.Core.Events.OnConfigurationChanged?.Invoke(this, CoreHandler.Core.Configuration);

            CoreHandler.Core.ConnectionHandler.OpenConnection();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
