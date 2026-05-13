using System;
using System.Globalization;
using Avalonia.Data.Converters;
using ConnectPro.Enums;

namespace ZenitelConnectProOperator.Converters;

/// <summary>
/// Converts CallState to a short, human-readable label for tooltips.
/// </summary>
public sealed class CallStateToLabelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CallState state)
            return "Unknown";

        return state switch
        {
            CallState.reachable => "Reachable",
            CallState.ringing => "Ringing",
            CallState.in_call => "In Call",
            CallState.queued => "Queued",
            CallState.init => "Initializing",
            CallState.forwarding => "Forwarding",
            CallState.fault => "Fault",
            CallState.ended => "Ended",
            _ => "Unknown"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns true if the CallState represents an active call (not reachable/ended/fault).
/// </summary>
public sealed class CallStateIsActiveConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CallState state)
            return false;

        return state is CallState.ringing or CallState.in_call or CallState.queued or CallState.init or CallState.forwarding;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns the appropriate call button text based on CallState.
/// </summary>
public sealed class CallStateToButtonTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CallState state)
            return "Call";

        return state switch
        {
            CallState.reachable or CallState.ended => "Call",
            _ => "End"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
