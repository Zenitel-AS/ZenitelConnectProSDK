using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ConnectPro.Enums;

namespace ZenitelConnectProOperator.Converters;

/// <summary>
/// Converts GpioState to a color for the indicator dot.
/// Active = colored (blue for GPI, amber for GPO), Inactive = gray outline
/// </summary>
public sealed class GpioStateToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush ActiveBlue = new(Color.Parse("#3B82F6"));
    private static readonly SolidColorBrush InactiveGray = new(Color.Parse("#4B5563"));
    private static readonly SolidColorBrush UnknownGray = new(Color.Parse("#374151"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not GpioState state)
            return UnknownGray;

        return state switch
        {
            GpioState.Active => ActiveBlue,
            GpioState.Inactive => InactiveGray,
            _ => UnknownGray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts GpioDirection to a color. GPI = Blue, GPO = Amber
/// </summary>
public sealed class GpioDirectionToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush GpiBlue = new(Color.Parse("#3B82F6"));
    private static readonly SolidColorBrush GpoAmber = new(Color.Parse("#F59E0B"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not GpioDirection direction)
            return GpiBlue;

        return direction == GpioDirection.Gpo ? GpoAmber : GpiBlue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns opacity based on GpioState - Active = full, Inactive = clearly visible but slightly dimmed
/// </summary>
public sealed class GpioStateToOpacityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GpioState state && state == GpioState.Active)
            return 1.0;
        return 0.7;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns true if GpioDirection is Input (GPI)
/// </summary>
public sealed class GpioDirectionIsInputConverter : IValueConverter
{
    public static readonly GpioDirectionIsInputConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GpioDirection direction)
            return direction == GpioDirection.Gpi;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns true if GpioDirection is Output (GPO)
/// </summary>
public sealed class GpioDirectionIsOutputConverter : IValueConverter
{
    public static readonly GpioDirectionIsOutputConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GpioDirection direction)
            return direction == GpioDirection.Gpo;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
