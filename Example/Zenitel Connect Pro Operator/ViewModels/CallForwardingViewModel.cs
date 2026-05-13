using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConnectPro.Models;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.ViewModels;

public partial class CallForwardingViewModel : ObservableObject, IDisposable
{
    private readonly IConnectProService? _connectPro;

    public ObservableCollection<CallForwardingRuleViewModel> Rules { get; } = new();

    [ObservableProperty] private string _newDirno = string.Empty;
    [ObservableProperty] private string _newFwdTo = string.Empty;
    [ObservableProperty] private int _newFwdTypeIndex;
    [ObservableProperty] private bool _newEnabled = true;
    [ObservableProperty] private bool _isBusy;

    public string[] FwdTypeOptions { get; } = new[] { "unconditional", "on_busy", "on_timeout" };

    // Design-time / fallback
    public CallForwardingViewModel()
    {
        Rules.Add(new CallForwardingRuleViewModel(new CallForwardingRule
        {
            Dirno = "1000",
            FwdType = "unconditional",
            FwdTo = "1010",
            Enabled = true
        }));
        Rules.Add(new CallForwardingRuleViewModel(new CallForwardingRule
        {
            Dirno = "1000",
            FwdType = "on_busy",
            FwdTo = "1010",
            Enabled = true
        }));
    }

    public CallForwardingViewModel(IConnectProService connectPro)
    {
        _connectPro = connectPro;

        _connectPro.Core.Events.OnConnectionChanged += OnConnectionChanged;
        _connectPro.Core.Events.OnCallForwardingRulesChange += OnRulesChanged;

        RefreshRules();
    }

    private void OnConnectionChanged(object? sender, bool e) =>
        Avalonia.Threading.Dispatcher.UIThread.Post(RefreshRules);

    private void OnRulesChanged(object? sender, EventArgs e) =>
        Avalonia.Threading.Dispatcher.UIThread.Post(RefreshRules);

    [RelayCommand]
    private void Refresh() => RefreshRules();

    private void RefreshRules()
    {
        if (_connectPro is null)
            return;

        var rules = _connectPro.Core.Collection.CallForwardingRules?
            .OrderBy(r => r.Dirno)
            .ThenBy(r => r.FwdType)
            .ToList() ?? new();

        Rules.Clear();
        foreach (var rule in rules)
            Rules.Add(new CallForwardingRuleViewModel(rule));
    }

    [RelayCommand]
    private async Task RetrieveFromServer()
    {
        if (_connectPro is null)
            return;

        IsBusy = true;
        try
        {
            await _connectPro.Core.CallForwardingHandler.RetrieveCallForwardingRules();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddRule()
    {
        if (_connectPro is null)
            return;

        if (string.IsNullOrWhiteSpace(NewDirno) || string.IsNullOrWhiteSpace(NewFwdTo))
            return;

        IsBusy = true;
        try
        {
            var rule = new CallForwardingRule
            {
                Dirno = NewDirno.Trim(),
                FwdType = FwdTypeOptions[NewFwdTypeIndex],
                FwdTo = NewFwdTo.Trim(),
                Enabled = NewEnabled
            };

            var success = await _connectPro.Core.CallForwardingHandler.AddOrUpdateCallForwardingRule(rule);
            if (success)
            {
                NewDirno = string.Empty;
                NewFwdTo = string.Empty;
                NewFwdTypeIndex = 0;
                NewEnabled = true;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteRule(CallForwardingRuleViewModel? ruleVm)
    {
        if (_connectPro is null || ruleVm is null)
            return;

        IsBusy = true;
        try
        {
            await _connectPro.Core.CallForwardingHandler.DeleteCallForwardingRules(
                ruleVm.Dirno, ruleVm.FwdType);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleEnabled(CallForwardingRuleViewModel? ruleVm)
    {
        if (_connectPro is null || ruleVm is null)
            return;

        IsBusy = true;
        try
        {
            var updatedRule = new CallForwardingRule
            {
                Dirno = ruleVm.Dirno,
                FwdType = ruleVm.FwdType,
                FwdTo = ruleVm.FwdTo,
                Enabled = !ruleVm.Enabled
            };

            await _connectPro.Core.CallForwardingHandler.AddOrUpdateCallForwardingRule(updatedRule);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Dispose()
    {
        if (_connectPro is not null)
        {
            _connectPro.Core.Events.OnConnectionChanged -= OnConnectionChanged;
            _connectPro.Core.Events.OnCallForwardingRulesChange -= OnRulesChanged;
        }
    }
}

public partial class CallForwardingRuleViewModel : ObservableObject
{
    public CallForwardingRuleViewModel(CallForwardingRule rule)
    {
        Rule = rule;
    }

    public CallForwardingRule Rule { get; }

    public string Dirno => Rule.Dirno;
    public string FwdType => Rule.FwdType;
    public string FwdTo => Rule.FwdTo;
    public bool Enabled => Rule.Enabled;

    public string FwdTypeDisplay => Rule.FwdType switch
    {
        "unconditional" => "Unconditional",
        "on_busy" => "On Busy",
        "on_timeout" => "On Timeout",
        _ => Rule.FwdType
    };

    public string EnabledDisplay => Rule.Enabled ? "Yes" : "No";

    public string ToggleButtonText => Rule.Enabled ? "Disable" : "Enable";
}
