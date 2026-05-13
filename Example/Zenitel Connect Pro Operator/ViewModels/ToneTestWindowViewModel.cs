using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ZenitelConnectProOperator.ViewModels;

public partial class ToneTestWindowViewModel : ObservableObject
{
    // Keep same default as WPF sample: tone group "3". :contentReference[oaicite:2]{index=2}
    private string _toneGroup = "3";

    public event EventHandler? RequestClose;

    // Full list + filtered list (mirrors your WPF pattern). :contentReference[oaicite:3]{index=3}
    public ObservableCollection<ToneTestDeviceRowViewModel> AllDevices { get; } = new();
    public ObservableCollection<ToneTestDeviceRowViewModel> FilteredDevices { get; } = new();

    public ObservableCollection<string> StatusOptions { get; } =
        new() { "All", "Pending", "Failed", "Passed" };

    public ObservableCollection<string> TestOptions { get; } =
        new()
        {
            "Test All Devices",
            "Test Checked Devices",
            "Test Failed Devices",
            "Test Passed Devices"
        };

    [ObservableProperty] private string searchQuery = string.Empty;
    [ObservableProperty] private string selectedStatus = "All";
    [ObservableProperty] private string selectedTestOption = "Test Checked Devices";
    [ObservableProperty] private string summaryText = "Devices in view: 0. Checked: 0. Passed: 0. Failed: 0. Pending: 0";

    [ObservableProperty] private bool isBusy;

    public bool CanRun => !IsBusy;

    // "Select All" checkbox in header
    private bool? _selectAll;
    public bool? SelectAll
    {
        get => _selectAll;
        set
        {
            if (SetProperty(ref _selectAll, value))
            {
                var newValue = value ?? false;
                foreach (var row in FilteredDevices)
                    row.IsChecked = newValue;

                UpdateSummary();
            }
        }
    }

    public ToneTestWindowViewModel()
    {
        // Demo-safe placeholder population.
        // Replace later with real CoreHandler/CoreRuntime data. :contentReference[oaicite:4]{index=4}
        PopulateDevicesPlaceholder();
        ApplyFilter();
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();
    partial void OnSelectedStatusChanged(string value) => ApplyFilter();

    [RelayCommand]
    private void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private async Task RunSelectedTest()
    {
        try
        {
            IsBusy = true;
            OnPropertyChanged(nameof(CanRun));

            // This mirrors the WPF selection logic. :contentReference[oaicite:5]{index=5}
            // TODO: wire to the real SDK:
            // CoreHandler.Core.DeviceHandler.InitiateToneTest(dirno, _toneGroup);

            await Task.Delay(150); // simulate work

            // Demo: mark rows as Pending to show UI response.
            if (SelectedTestOption == "Test All Devices")
            {
                foreach (var row in AllDevices)
                    row.SetStatus("Pending");
            }
            else if (SelectedTestOption == "Test Checked Devices")
            {
                foreach (var row in AllDevices.Where(x => x.IsChecked))
                    row.SetStatus("Pending");
            }
            else if (SelectedTestOption == "Test Failed Devices")
            {
                foreach (var row in AllDevices.Where(x => x.CurrentStatus == "Failed"))
                    row.SetStatus("Pending");
            }
            else if (SelectedTestOption == "Test Passed Devices")
            {
                foreach (var row in AllDevices.Where(x => x.CurrentStatus == "Passed"))
                    row.SetStatus("Pending");
            }

            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(CanRun));
        }
    }

    private void ApplyFilter()
    {
        var filtered = AllDevices.AsEnumerable();

        var q = (SearchQuery ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var qLower = q.ToLowerInvariant();
            filtered = filtered.Where(d =>
                (d.Dirno ?? string.Empty).ToLowerInvariant().Contains(qLower) ||
                (d.Name ?? string.Empty).ToLowerInvariant().Contains(qLower));
        }

        if (!string.Equals(SelectedStatus, "All", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(d =>
                string.Equals(d.CurrentStatus, SelectedStatus, StringComparison.OrdinalIgnoreCase));
        }

        FilteredDevices.Clear();
        foreach (var item in filtered)
            FilteredDevices.Add(item);

        // Keep SelectAll state consistent with current filtered set.
        SelectAll = FilteredDevices.Count == 0
            ? false
            : FilteredDevices.All(x => x.IsChecked);

        UpdateSummary();
    }

    private void UpdateSummary()
    {
        var total = FilteredDevices.Count;
        var checkedCount = FilteredDevices.Count(d => d.IsChecked);
        var passedCount = FilteredDevices.Count(d => d.CurrentStatus == "Passed");
        var failedCount = FilteredDevices.Count(d => d.CurrentStatus == "Failed");
        var pendingCount = FilteredDevices.Count(d => d.CurrentStatus == "Pending");

        SummaryText = $"Devices in view: {total}.  Checked: {checkedCount}.  Passed: {passedCount}.  Failed: {failedCount}.  Pending: {pendingCount}";
    }

    private void PopulateDevicesPlaceholder()
    {
        // Replace with: CoreHandler.Core.Collection.RegisteredDevices... :contentReference[oaicite:6]{index=6}
        AllDevices.Clear();

        AllDevices.Add(new ToneTestDeviceRowViewModel("1001", "Entrance A", "Not Tested"));
        AllDevices.Add(new ToneTestDeviceRowViewModel("1002", "Entrance B", "Failed"));
        AllDevices.Add(new ToneTestDeviceRowViewModel("1003", "Gate West", "Passed"));
        AllDevices.Add(new ToneTestDeviceRowViewModel("1004", "Platform 1", "Pending"));

        foreach (var row in AllDevices)
            row.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ToneTestDeviceRowViewModel.IsChecked))
                    UpdateSummary();
            };
    }
}

public partial class ToneTestDeviceRowViewModel : ObservableObject
{
    // WPF model is Device + ExtendedStatus + IsChecked. :contentReference[oaicite:7]{index=7}
    // Here we flatten for DataGrid readability (demo app style).

    public ToneTestDeviceRowViewModel(string dirno, string name, string status)
    {
        Dirno = dirno;
        Name = name;
        CurrentStatus = status;
    }

    [ObservableProperty] private string? dirno;
    [ObservableProperty] private string? name;

    [ObservableProperty] private string currentStatus = "Not Tested";
    [ObservableProperty] private string? lastPass;
    [ObservableProperty] private string? lastFail;

    [ObservableProperty] private bool isChecked;

    public void SetStatus(string status)
    {
        CurrentStatus = status;

        if (status == "Passed")
            LastPass = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        if (status == "Failed")
            LastFail = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
