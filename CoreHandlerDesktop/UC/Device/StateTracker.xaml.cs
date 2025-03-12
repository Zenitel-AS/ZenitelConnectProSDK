using ConnectPro.Enums;
using ConnectPro.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace CoreHandlerDesktop.UC.Device
{
    public partial class StateTracker : Window, INotifyPropertyChanged
    {
        public StateTracker()
        {
            InitializeComponent();
            DataContext = this;  // ✅ Enable data binding

            DirectoryNumbers = new ObservableCollection<string>(CoreHandler.Core.Collection.RegisteredDevices.Select(d => d.dirno)); // ✅ Extract only dirno

            CoreHandler.Core.Events.OnDeviceRetrievalEnd += HandleDeviceListRetrieved;
        }

        private string _selectedDeviceDirno = "";
        public string SelectedDeviceDirno
        {
            get { return _selectedDeviceDirno; }
            set
            {
                _selectedDeviceDirno = value;
                SelectedDevice = CoreHandler.Core.Collection.RegisteredDevices.FirstOrDefault(d => d.dirno == value); // ✅ Ensure casing matches
                OnPropertyChanged(nameof(SelectedDeviceDirno));

                SelectedDevice.OnCallStateChange += HandleDeviceCallStateChange;
            }
        }

        public ConnectPro.Models.Device SelectedDevice { get; set; }

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
                RefreshDirectoryNumbers();
            });
        }

        private void HandleDeviceCallStateChange(object sender, CallState callState)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ConnectPro.Models.Device device = (ConnectPro.Models.Device)sender;
                DeviceListView.Items.Insert(0, $"{DateTime.Now.ToShortTimeString()} - {device.dirno} - Device state: {device.DeviceState} - Call state: {device.CallState}");
            });
        }

        private void RefreshDirectoryNumbers()
        {
            DirectoryNumbers.Clear();
            foreach (var device in CoreHandler.Core.Collection.RegisteredDevices)
            {
                DirectoryNumbers.Add(device.dirno); // ✅ Ensure only DirNo values
            }
            OnPropertyChanged(nameof(DirectoryNumbers)); // ✅ Notify UI
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
