using ConnectPro.Handlers;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WampSharp.Newtonsoft;

namespace CoreHandlerDesktop.UC
{
    /// <summary>
    /// Interaction logic for IO.xaml
    /// </summary>
    public partial class IO : UserControl
    {
        // Dependency Property for Device binding
        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register(nameof(Device), typeof(ConnectPro.Models.Device), typeof(IO), new PropertyMetadata(null));

        public ConnectPro.Models.Device Device
        {
            get => (ConnectPro.Models.Device)Dispatcher.Invoke(() => GetValue(DeviceProperty)); // ✅ Ensure UI thread access
            set => Dispatcher.Invoke(() => SetValue(DeviceProperty, value)); // ✅ Ensure UI thread access
        }

        // ✅ Default constructor (Required by WPF)
        public IO()
        {
            InitializeComponent();
            CoreHandler.Core.Events.OnDoorOpen += HandleDoorOpen;
        }

        private void HandleDoorOpen(object sender, bool success)
        {
            if (sender != null && sender is AccessControlHandler)
            {
                if (Device.dirno == (sender as AccessControlHandler).OpenDoorEventData.DoorDirno)
                {
                    txtIO.Dispatcher.Invoke(() =>
                    {
                        txtIO.Text = success
                            ? $"Door opened!"
                            : $"Failed.";
                    });
                    MessageBox.Show($"Door open! From dirno: {(sender as AccessControlHandler).OpenDoorEventData.FromDirno} Door dirno: {(sender as AccessControlHandler).OpenDoorEventData.DoorDirno}");
                }
            }
        }

        private void OpenDoor_Click(object sender, RoutedEventArgs e)
        {
            if (Device != null)
            {
                CoreHandler.Core.AccessControlHandler.OpenDoor(Device, CoreHandler.Core.Configuration.OperatorDirNo);
            }
        }

        private void btnStartCall_Click(object sender, RoutedEventArgs e)
        {
            if (Device != null)
            {
                Task.Run(async () => await CoreHandler.Core.CallHandler.PostCall(CoreHandler.Core.Configuration.OperatorDirNo, Device.dirno, "setup"));
            }
        }

        private void btnDropCall_Click(object sender, RoutedEventArgs e)
        {
            if (Device != null)
            {
                Task.Run(async () => await CoreHandler.Core.CallHandler.DeleteCall(Device.dirno));
            }
        }
    }
}
