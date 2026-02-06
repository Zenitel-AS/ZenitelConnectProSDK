using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ZenitelConnectProOperator.Converters;

public sealed class LoadedButEmptyToVisibleConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return AvaloniaProperty.UnsetValue;

        var loaded = values[0] is bool b0 && b0;
        var has = values[1] is bool b1 && b1;

        return loaded && !has;
    }
}
