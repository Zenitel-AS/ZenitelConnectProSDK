using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CoreHandlerDesktop.UC
{
    /// <summary>
    /// Interaction logic for ConnectionStatus.xaml
    /// </summary>
    public partial class ConnectionStatus : UserControl, INotifyPropertyChanged
    {
        private string _connectionMessage;
        private Brush _connectionColor;

        public event PropertyChangedEventHandler PropertyChanged;

        public string ConnectionMessage
        {
            get => _connectionMessage;
            set
            {
                _connectionMessage = value;
                OnPropertyChanged(nameof(ConnectionMessage));
            }
        }

        public Brush ConnectionColor
        {
            get => _connectionColor;
            set
            {
                _connectionColor = value;
                OnPropertyChanged(nameof(ConnectionColor));
            }
        }

        public ConnectionStatus()
        {
            InitializeComponent();
            DataContext = this; // Set the DataContext to this instance
            CoreHandler.Core.Events.OnConnectionChanged += ConnectionHandler_OnConnectionStatusChanged;
            CoreHandler.Core.ConnectionHandler.OpenConnection(); // Initialize with default values
        }

        private void ConnectionHandler_OnConnectionStatusChanged(object sender, bool isConnected)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateConnectionStatus(isConnected);
            });
        }

        private void UpdateConnectionStatus(bool isConnected)
        {
            ConnectionMessage = isConnected ? "Connected" : "Disconnected";
            ConnectionColor = isConnected ? Brushes.Green : Brushes.Red;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
