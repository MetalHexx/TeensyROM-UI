using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace TeensyRom.Ui.Helpers
{
    public class BoolToVisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
            {
                return Visibility.Collapsed;
            }
            var objValue = (bool)value;

            if (objValue)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                {
                    return Visibility.Visible;
                }
            }
            catch (Exception)
            {
            }
            return Visibility.Collapsed;
        }
    }
}