using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ConnectPro.Models;

namespace CoreHandlerDesktop.UC
{
    /// <summary>
    /// Interaction logic for Groups.xaml
    /// </summary>
    public partial class Groups : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<Group> _groupList;
        private Group _selectedGroup;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Group> GroupList
        {
            get => _groupList;
            set
            {
                _groupList = value;
                OnPropertyChanged(nameof(GroupList));
            }
        }

        public Group SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                _selectedGroup = value;
                OnPropertyChanged(nameof(SelectedGroup));
            }
        }

        public Groups()
        {
            InitializeComponent();
            DataContext = this;
            GroupList = new ObservableCollection<Group>(); // Initialize the collection
            CoreHandler.Core.Events.OnGroupsListChange += HandleGroupListChange;
        }

        public void UpdateGroups(IEnumerable<Group> groups)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GroupList.Clear();
                foreach (var group in groups)
                {
                    GroupList.Add(group);
                }
            });
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HandleGroupListChange(object sender, EventArgs eventArgs)
        {
            UpdateGroups(CoreHandler.Core.Collection.Groups);
        }
    }
}
