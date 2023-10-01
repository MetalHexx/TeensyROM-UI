using System;
using System.Globalization;
using System.Windows.Data;

namespace TeensyRom.Ui.Helpers
{
    /// <summary>
    /// This convertor will take a boolean value and inverse it for the UI xaml
    /// </summary>
    public class BooleanInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return Binding.DoNothing;
        }
    }
}