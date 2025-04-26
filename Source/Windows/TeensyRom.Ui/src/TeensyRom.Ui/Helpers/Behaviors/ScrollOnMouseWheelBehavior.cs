using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace TeensyRom.Ui.Helpers.Behaviors
{
    public static class ScrollOnMouseWheelBehavior
    {
        public static readonly DependencyProperty EnableScrollOnMouseWheelProperty =
            DependencyProperty.RegisterAttached(
                "EnableScrollOnMouseWheel",
                typeof(bool),
                typeof(ScrollOnMouseWheelBehavior),
                new PropertyMetadata(false, OnEnableScrollOnMouseWheelChanged));

        public static void SetEnableScrollOnMouseWheel(DependencyObject element, bool value)
        {
            element.SetValue(EnableScrollOnMouseWheelProperty, value);
        }

        public static bool GetEnableScrollOnMouseWheel(DependencyObject element)
        {
            return (bool)element.GetValue(EnableScrollOnMouseWheelProperty);
        }

        private static void OnEnableScrollOnMouseWheelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                    element.PreviewMouseWheel += ElementOnPreviewMouseWheel;
                else
                    element.PreviewMouseWheel -= ElementOnPreviewMouseWheel;
            }
        }

        private static void ElementOnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not DependencyObject dependencyObject) return;

            var scrollViewer = FindParent<ScrollViewer>(dependencyObject);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parent = VisualTreeHelper.GetParent(child);
            while (parent != null && parent is not T)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }
}