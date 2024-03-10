using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;

namespace TeensyRom.Ui.Helpers
{
    public class StringToPackIconKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string iconName && !string.IsNullOrEmpty(iconName))
            {
                if (Enum.TryParse<PackIconKind>(iconName, out var iconKind))
                {
                    return iconKind;
                }
            }

            // Return a default icon if the input is null/empty or not a valid icon name
            return PackIconKind.Alert; // Choose an appropriate fallback icon
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}