using ConnectPro.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoreHandlerDesktop.UC.Calls
{
    /// <summary>
    /// Interaction logic for CallTracker.xaml
    /// </summary>
    public partial class CallTracker : Window
    {
        public CallTracker()
        {
            InitializeComponent();
            DataContext = this;  // ✅ Enable data binding

            CoreHandler.Core.Events.OnCallEvent += HandleCallChange;
        }

        private ObservableCollection<CallElement> _callList = new ObservableCollection<CallElement>();
        public ObservableCollection<CallElement> CallList
        {
            get => _callList;
            set
            {
                _callList = value;
                OnPropertyChanged(nameof(CallList));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void HandleCallChange(object sender, CallElement call)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CallList.Insert(0, call);  // Insert the whole CallElement object
            });
        }

    }
}
