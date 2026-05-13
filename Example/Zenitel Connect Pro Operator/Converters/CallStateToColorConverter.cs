using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ConnectPro.Enums;

namespace ZenitelConnectProOperator.Converters;

/// <summary>
/// Converts CallState to a color for the status indicator dot.
/// Reachable = Green, Calling states = Blue, Fault = Red
/// </summary>
public sealed class CallStateToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush Green = new(Color.Parse("#22C55E"));
    private static readonly SolidColorBrush Blue = new(Color.Parse("#3B82F6"));
    private static readonly SolidColorBrush Red = new(Color.Parse("#EF4444"));
    private static readonly SolidColorBrush Gray = new(Color.Parse("#6B7280"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CallState state)
            return Gray;

        return state switch
        {
            CallState.reachable => Green,
            CallState.ringing or CallState.in_call or CallState.queued or CallState.init or CallState.forwarding => Blue,
            CallState.fault => Red,
            CallState.ended => Gray,
            _ => Gray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
