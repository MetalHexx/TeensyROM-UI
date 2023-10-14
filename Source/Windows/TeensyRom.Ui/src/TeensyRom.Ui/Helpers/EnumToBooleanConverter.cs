using System;
using System.Globalization;
using System.Windows.Data;

namespace TeensyRom.Ui.Helpers
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string enumValueString = value.ToString();
            string targetValueString = parameter.ToString();
            return enumValueString.Equals(targetValueString, StringComparison.OrdinalIgnoreCase);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            bool useValue = (bool)value;
            string targetValueString = parameter.ToString();
            return useValue ? Enum.Parse(targetType, targetValueString) : Binding.DoNothing;
        }
    }
}