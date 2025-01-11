using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace SpeedTest.Converters;

public class SpeedToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double speed)
        {
            // Define color ranges based on speed
            if (speed < 10) return new SolidColorBrush(Color.Parse("#ff4444")); // Red for slow
            if (speed < 30) return new SolidColorBrush(Color.Parse("#ffbb33")); // Orange for medium
            if (speed < 50) return new SolidColorBrush(Color.Parse("#00C851")); // Green for fast
            return new SolidColorBrush(Color.Parse("#33b5e5")); // Blue for very fast
        }

        return new SolidColorBrush(Color.Parse("#333333")); // Default gray
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}