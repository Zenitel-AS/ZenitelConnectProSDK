using ConnectPro.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CoreHandlerDesktop.UC
{
    /// <summary>
    /// Interaction logic for Logs.xaml
    /// </summary>
    public partial class LogsWindow : Window
    {
        private readonly Dictionary<string, ObservableCollection<object>> _logs;

        public LogsWindow()
        {
            InitializeComponent();
            _logs = new Dictionary<string, ObservableCollection<object>>();
            LogTypes.ItemsSource = new List<string> { "WAMP", "Error", "Warning", "Info" };
            LogTypes.SelectedIndex = 0;
            CoreHandler.Core.Events.OnChildLogEntry += OnLogReceived;
        }

        public void AddLogEntry(string logType, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return; // Skip empty logs

            if (!_logs.ContainsKey(logType))
            {
                _logs[logType] = new ObservableCollection<object>();
            }

            object processedMessage = ProcessLogMessage(logType, message);
            if (processedMessage != null)
            {
                _logs[logType].Insert(0, processedMessage); // Newest logs first
            }

            if ((string)LogTypes.SelectedItem == logType)
            {
                LogList.ItemsSource = _logs[logType];
            }
        }

        private object ProcessLogMessage(string logType, string message)
        {
            // Mask access token
            if (message.Contains("Access Token:"))
            {
                message = Regex.Replace(message, @"(Access Token: \S{10})\S+", "$1... (hidden)");
            }

            // Try to extract JSON from message
            string extractedJson = ExtractJson(message, out string title);
            if (!string.IsNullOrWhiteSpace(extractedJson))
            {
                try
                {
                    var jsonObject = JToken.Parse(extractedJson);
                    if (!jsonObject.HasValues) return message; // Display as text if JSON is empty
                    return ConvertJsonToTree(title ?? logType, jsonObject);
                }
                catch (Exception)
                {
                    return message;
                }
            }

            return message; // Default to plain text if no valid JSON is found
        }

        private string ExtractJson(string input, out string title)
        {
            title = null;
            int startIndex = input.IndexOfAny(new char[] { '{', '[' });
            int endIndex = input.LastIndexOfAny(new char[] { '}', ']' });

            if (startIndex != -1 && endIndex > startIndex)
            {
                title = input.Substring(0, startIndex).Trim(); // Extract title
                return input.Substring(startIndex, endIndex - startIndex + 1);
            }

            return string.Empty; // Return empty if no valid JSON is found
        }

        private Border ConvertJsonToTree(string title, JToken jsonObject)
        {
            TreeView treeView = new TreeView
            {
                Background = Brushes.Transparent,
                ContextMenu = CreateContextMenu()
            };

            TreeViewItem rootNode = new TreeViewItem
            {
                Header = string.IsNullOrEmpty(title) ? "JSON Data" : title,
                IsExpanded = false // Start as collapsed
            };
            treeView.Items.Add(rootNode);
            PopulateTree(jsonObject, rootNode);

            // Attach scrolling fix
            treeView.PreviewMouseWheel += TreeView_PreviewMouseWheel;

            return new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Child = treeView
            };
        }

        private void PopulateTree(JToken token, ItemsControl parent)
        {
            if (token is JProperty property)
            {
                TreeViewItem node = new TreeViewItem { Header = property.Name, IsExpanded = false };
                parent.Items.Add(node);

                if (property.Value is JValue)
                {
                    node.Items.Add(new TreeViewItem { Header = property.Value.ToString() });
                }
                else
                {
                    PopulateTree(property.Value, node);
                }
            }
            else if (token is JObject obj)
            {
                foreach (var prop in obj.Properties())
                {
                    PopulateTree(prop, parent);
                }
            }
            else if (token is JArray array)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    TreeViewItem node = new TreeViewItem { Header = $"[{i}]", IsExpanded = false };
                    parent.Items.Add(node);
                    PopulateTree(array[i], node);
                }
            }
            else if (token is JValue value)
            {
                parent.Items.Add(new TreeViewItem { Header = value.ToString() });
            }
        }

        private ContextMenu CreateContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem copyItem = new MenuItem { Header = "Copy" };
            copyItem.Click += CopyToClipboard;
            contextMenu.Items.Add(copyItem);
            return contextMenu;
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            if (LogList.SelectedItem != null)
            {
                Clipboard.SetText(LogList.SelectedItem.ToString());
            }
        }

        public event Action<string, string> LogReceived;

        public void OnLogReceived(object sender, string message)
        {
            Application.Current.Dispatcher.Invoke(() => AddLogEntry("WAMP", message));
        }

        private void LogTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedLogType = (string)LogTypes.SelectedItem;
            LogList.ItemsSource = _logs.ContainsKey(selectedLogType) ? _logs[selectedLogType] : new ObservableCollection<object>();
        }

        private void TreeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var parent = ((Control)sender).Parent as UIElement;
                while (parent != null && !(parent is ScrollViewer))
                {
                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }

                if (parent is ScrollViewer scrollViewer)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                }
            }
        }
    }
}
