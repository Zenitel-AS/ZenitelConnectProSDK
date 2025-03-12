using CoreHandlerDesktop.UC;
using CoreHandlerDesktop.UC.Calls;
using CoreHandlerDesktop.UC.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZenitelConnectMIP.UIElements;

namespace CoreHandlerDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConfigurationWindow configWindow = new ConfigurationWindow();
        List<StateTracker> stateTrackers = new List<StateTracker>();
        public MainWindow()
        {
            InitializeComponent();
            // Make it fullscreen
            this.WindowState = WindowState.Maximized;
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            if (configWindow.IsVisible == false)
            {
                try
                {
                    configWindow.Topmost = true;
                    configWindow.Show();
                }
                catch (System.InvalidOperationException IOE)
                {
                    configWindow.Topmost = true;
                    configWindow.Activate();
                }
            }
        }

        private void btnStateWindow_Click(object sender, RoutedEventArgs e)
        {
            var stateTrackingWin = new StateTracker()
            {
                Topmost = true
            };
            stateTrackingWin.Show();
            stateTrackers.Add(stateTrackingWin);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (configWindow != null && configWindow.IsVisible)
            {
                configWindow.Close();
            }

            foreach (StateTracker trackerWin in stateTrackers)
            {
                trackerWin.Close();
            }

            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit?", "Exit Confirmation",
                                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true; // Prevent window from closing
            }

            Application.Current.Shutdown();
        }

        private void btnCallTrackerWindow_Click(object sender, RoutedEventArgs e)
        {
            new CallTracker()
            {
                Topmost = true
            }.Show();
        }

        private void btnLogsWindow_Click(object sender, RoutedEventArgs e)
        {
            new LogsWindow()
            {
                Topmost = true
            }.Show();
        }
    }
}
