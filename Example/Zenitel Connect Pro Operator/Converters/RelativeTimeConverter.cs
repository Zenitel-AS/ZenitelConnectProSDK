using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ZenitelConnectProOperator.Converters;

/// <summary>
/// Converts DateTimeOffset to a relative time string (e.g., "Just now", "5s ago", "2m ago").
/// Absolute time is only shown for times older than 1 hour.
/// </summary>
public sealed class RelativeTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTimeOffset timestamp)
            return string.Empty;

        var elapsed = DateTimeOffset.UtcNow - timestamp;

        if (elapsed.TotalSeconds < 5)
            return "Just now";

        if (elapsed.TotalSeconds < 60)
            return $"{(int)elapsed.TotalSeconds}s ago";

        if (elapsed.TotalMinutes < 60)
            return $"{(int)elapsed.TotalMinutes}m ago";

        if (elapsed.TotalHours < 24)
            return $"{(int)elapsed.TotalHours}h ago";

        if (elapsed.TotalDays < 7)
            return $"{(int)elapsed.TotalDays}d ago";

        // Fallback to short date for older timestamps
        return timestamp.LocalDateTime.ToString("MMM d", culture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
