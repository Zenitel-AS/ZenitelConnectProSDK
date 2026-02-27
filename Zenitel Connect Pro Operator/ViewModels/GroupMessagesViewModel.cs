using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectPro.Models;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.ViewModels;

public partial class GroupMessagesViewModel : ObservableObject
{
    private readonly Group _group;
    private readonly IConnectProService? _connectPro;

    public event EventHandler? RequestClose;

    public string GroupName { get; }
    public string GroupDirNo { get; }

    public ObservableCollection<AudioMessageRowViewModel> AudioMessages { get; } = new();

    // Repeat flags (mutually exclusive like WPF) :contentReference[oaicite:19]{index=19}
    [ObservableProperty] private bool noRepeat = true;
    [ObservableProperty] private bool infiniteRepeat;
    [ObservableProperty] private bool isLimitedRepeat;
    [ObservableProperty] private int repeatNumber = 1;

    public GroupMessagesViewModel(Group group)
    {
        _group = group;
        GroupName = group.DisplayName;
        GroupDirNo = group.Dirno;
    }

    public GroupMessagesViewModel(Group group, IConnectProService connectPro) : this(group)
    {
        _connectPro = connectPro;
        _connectPro.Core.Events.OnAudioMessagesChange += OnAudioMessagesChanged;

        // Populate immediately if audio messages are already loaded
        PopulateMessageList();
    }

    private void OnAudioMessagesChanged(object? sender, bool e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(PopulateMessageList);
    }

    private void PopulateMessageList()
    {
        var msgs = _connectPro?.Core.Collection.AudioMessages?.ToList() ?? new();
        AudioMessages.Clear();
        foreach (var m in msgs)
            AudioMessages.Add(new AudioMessageRowViewModel(m));
    }
    partial void OnNoRepeatChanged(bool value)
    {
        if (!value) return;
        InfiniteRepeat = false;
        IsLimitedRepeat = false;
        RepeatNumber = 1;
    }

    partial void OnInfiniteRepeatChanged(bool value)
    {
        if (!value) return;
        NoRepeat = false;
        IsLimitedRepeat = false;
    }

    partial void OnIsLimitedRepeatChanged(bool value)
    {
        if (!value) return;
        NoRepeat = false;
        InfiniteRepeat = false;
    }

    [RelayCommand]
    private void Close() => RequestClose?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void TogglePlay(AudioMessageRowViewModel row)
    {
        if (_connectPro is null)
            return;

        var msg = row.AudioMessage;

        if (row.IsPlaying)
        {
            _connectPro.Core.BroadcastingHandler?.StopAudioMessage(msg); // :contentReference[oaicite:20]{index=20}
            row.SetPlaying(false);
            return;
        }

        var repeatCount = InfiniteRepeat ? int.MaxValue : RepeatNumber;
        _connectPro.Core.BroadcastingHandler?.PlayAudioMessage(msg, _group.Dirno, "setup", repeatCount); // :contentReference[oaicite:21]{index=21}
        row.SetPlaying(true);
    }
}

public partial class AudioMessageRowViewModel : ObservableObject
{
    public AudioMessageRowViewModel(AudioMessage msg) => AudioMessage = msg;

    public AudioMessage AudioMessage { get; }

    public string? Dirno => AudioMessage.Dirno;
    public string? DisplayName => AudioMessage.DisplayName;

    [ObservableProperty] private bool isPlaying;

    public string PlayButtonText => IsPlaying ? "Stop" : "Play";

    public void SetPlaying(bool playing)
    {
        IsPlaying = playing;
        OnPropertyChanged(nameof(PlayButtonText));
    }
}
