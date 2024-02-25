using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TeensyRom.Ui.Helpers
{
    public class InverseNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                return string.IsNullOrWhiteSpace(value as string) ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value is null) return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}