using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace TeensyRom.Ui.Helpers
{
    public class InverseBoolToVisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
            {
                return Visibility.Visible;
            }
            bool objValue = (bool)value;

            if (objValue)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                {
                    return Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
            }
            return Visibility.Visible;
        }
    }
}