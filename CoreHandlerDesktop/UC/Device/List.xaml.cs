using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using System.Linq;

namespace CoreHandlerDesktop.UC
{
    /// <summary>
    /// Interaction logic for List.xaml
    /// </summary>
    public partial class List : UserControl
    {
        public ObservableCollection<ConnectPro.Models.Device> DeviceList { get; set; }

        public List()
        {
            InitializeComponent();
            DeviceList = new ObservableCollection<ConnectPro.Models.Device>(CoreHandler.Core.Collection.RegisteredDevices);
            DataContext = this;

            CoreHandler.Core.Events.OnDeviceRetrievalEnd += HandleDeviceListLoaded;
        }

        private void HandleDeviceListLoaded(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DeviceList.Clear();
                foreach (var device in CoreHandler.Core.Collection.RegisteredDevices)
                {
                    DeviceList.Add(device);
                    device.OnCallStateChange += HandleDeviceCallStateChange;
                }
            });
        }

        private void HandleDeviceCallStateChange(object sender, ConnectPro.Enums.CallState callState)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (sender != null)
                {
                    DeviceList.Remove((sender as ConnectPro.Models.Device));
                    DeviceList.Add((sender as ConnectPro.Models.Device));
                }
            });
        }
    }
}
