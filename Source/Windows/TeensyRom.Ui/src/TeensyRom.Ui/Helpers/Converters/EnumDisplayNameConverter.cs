using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace TeensyRom.Ui.Helpers
{
    public class EnumDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            var text = value.ToString()!;
            return Regex.Replace(text, "(\\B[A-Z])", " $1"); // Insert space before capital letters
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Enum.Parse(targetType, value.ToString()!.Replace(" ", ""));
    }
}