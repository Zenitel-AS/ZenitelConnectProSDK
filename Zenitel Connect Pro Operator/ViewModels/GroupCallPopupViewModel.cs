using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectPro.Models;
using System;
using System.Threading.Tasks;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.ViewModels;

public partial class GroupCallPopupViewModel : ObservableObject
{
    private readonly Group _group;
    private readonly GroupsViewModel _owner;
    private readonly IConnectProService? _connectPro;

    public event EventHandler? RequestClose;

    public string DirNo => _group.Dirno;
    public string DisplayName => _group.DisplayName;

    [ObservableProperty] private bool mute;

    public string MuteButtonText => !Mute ? "Unmute" : "Mute";

    // ownerVm is passed so we can clear busy state in the same list instance.
    public GroupCallPopupViewModel(Group group, GroupsViewModel ownerVm)
    {
        _group = group;
        _owner = ownerVm;
    }

    public GroupCallPopupViewModel(Group group, GroupsViewModel ownerVm, IConnectProService connectPro)
        : this(group, ownerVm)
    {
        _connectPro = connectPro;
    }

    [RelayCommand]
    private void ToggleMute()
    {
        if (_connectPro is null)
            return;

        var op = _connectPro.Core.Configuration.OperatorDirNo;
        if (string.IsNullOrWhiteSpace(op))
            return;

        var ok = _connectPro.Core.DeviceHandler.SimulateKeyPress(op, "m", "press");
        if (ok)
        {
            Mute = !Mute;
            OnPropertyChanged(nameof(MuteButtonText));
        }
    }

    [RelayCommand]
    private async Task EndCall()
    {
        if (_connectPro is not null)
        {
            // Safe-ish close path: stop the group call by group dirno
            // (avoids relying on GetAllCals signature).
            await _connectPro.Core.CallHandler.DeleteCall(_group.Dirno);
        }

        _group.IsBusy = false;
        _owner.RefreshCommand.Execute(null);

        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
