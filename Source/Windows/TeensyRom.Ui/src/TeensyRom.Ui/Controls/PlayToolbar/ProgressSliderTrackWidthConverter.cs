using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TeensyRom.Ui.Controls.PlayToolbar
{
    public class ProgressSliderTrackWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;

            if (values[0] is double progressValue && values[1] is double sliderWidth)
            {
                return progressValue * sliderWidth;
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
