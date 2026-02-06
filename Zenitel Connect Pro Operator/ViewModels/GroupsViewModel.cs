using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectPro.Models;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.ViewModels;

public partial class GroupsViewModel : ObservableObject, IDisposable
{
    private readonly IConnectProService? _connectPro;

    public ObservableCollection<GroupRowViewModel> Groups { get; } = new();

    public event EventHandler<Group>? RequestOpenGroupCallPopup;
    public event EventHandler<Group>? RequestOpenGroupMessages;

    // Design-time / fallback
    public GroupsViewModel()
    {
        Groups.Add(new GroupRowViewModel(new Group { Dirno = "2001", DisplayName = "All Hands" }));
        Groups.Add(new GroupRowViewModel(new Group { Dirno = "2002", DisplayName = "Security" }));
    }

    public GroupsViewModel(IConnectProService connectPro)
    {
        _connectPro = connectPro;

        _connectPro.Core.Events.OnConnectionChanged += OnConnectionChanged;
        _connectPro.Core.Events.OnGroupsListChange += (s, e) =>
        {
            // The intent is to trigger RefreshGroups safely on the UI thread;
            // Avalonia is merely the dispatch mechanism, not the focus or dependency here.
            Avalonia.Threading.Dispatcher.UIThread.Post(RefreshGroups);
        };

        RefreshGroups();
    }

    private void OnConnectionChanged(object? sender, bool e) => RefreshGroups();

    [RelayCommand]
    private void Refresh() => RefreshGroups();

    private void RefreshGroups()
    {
        if (_connectPro is null)
            return;

        var groups = _connectPro.Core.Collection.Groups?
            .Select(g =>
            {
                g.IsBusy = g.IsBusy; // preserve state
                g.BroadcastedMessageName ??= "Stored Message";
                return g;
            })
            .OrderBy(g => g.Dirno)
            .ToList() ?? new();

        Groups.Clear();
        foreach (var g in groups)
            Groups.Add(new GroupRowViewModel(g));
    }

    [RelayCommand]
    private async Task InitiateGroupCall(GroupRowViewModel row)
    {
        if (_connectPro is null)
            return;

        var op = _connectPro.Core.Configuration.OperatorDirNo;
        if (string.IsNullOrWhiteSpace(op))
            return;

        var group = row.Group;
        if (group is null)
            return;

        // Mirrors WPF: PostCall(operator, group.Dirno, "setup") then open popup and set IsBusy
        await _connectPro.Core.CallHandler.PostCall(op, group.Dirno, "setup");

        group.IsBusy = true;
        row.NotifyBusyChanged();

        RequestOpenGroupCallPopup?.Invoke(this, group);
    }

    [RelayCommand]
    private void OpenMessages(GroupRowViewModel row)
    {
        var group = row.Group;
        if (group is null)
            return;

        // Mirrors WPF: PopupHandler.OpenGroupMessageWindow(group)
        RequestOpenGroupMessages?.Invoke(this, group);
    }

    [RelayCommand]
    private void MPress()
    {
        if (_connectPro is null)
            return;

        var op = _connectPro.Core.Configuration.OperatorDirNo;
        if (string.IsNullOrWhiteSpace(op))
            return;

        _connectPro.Core.DeviceHandler.SimulateKeyPress(op, "M", "press");
    }

    [RelayCommand]
    private void MRelease()
    {
        if (_connectPro is null)
            return;

        var op = _connectPro.Core.Configuration.OperatorDirNo;
        if (string.IsNullOrWhiteSpace(op))
            return;

        _connectPro.Core.DeviceHandler.SimulateKeyPress(op, "M", "release");
    }

    public void Dispose()
    {
        if (_connectPro is not null)
        {
            _connectPro.Core.Events.OnConnectionChanged -= OnConnectionChanged;
        }
    }
}

public partial class GroupRowViewModel : ObservableObject
{
    public GroupRowViewModel(Group group) => Group = group;

    public Group Group { get; }

    public string DirNo => Group.Dirno;
    public string DisplayName => Group.DisplayName;

    public string BroadcastedMessageName
    {
        get => Group.BroadcastedMessageName;
        set
        {
            if (Group.BroadcastedMessageName != value)
            {
                Group.BroadcastedMessageName = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanInitiateCall => !Group.IsBusy;

    public void NotifyBusyChanged()
    {
        OnPropertyChanged(nameof(CanInitiateCall));
    }
}
