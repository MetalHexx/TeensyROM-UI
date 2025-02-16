using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace TeensyRom.Ui.Controls.PlayToolbar
{
    /// <summary>
    /// Interaction logic for PlayToolbarView.xaml
    /// </summary>
    public partial class PlayToolbarView : UserControl
    {
        private bool _isAdvancedVisible = false;
        private Binding? _progressSliderBinding;
        private bool _progressMouseDowned = false;

        public PlayToolbarView()
        {
            InitializeComponent();
            Loaded += PlayToolbarView_Loaded;
        }

        private void PlayToolbarView_Loaded(object sender, RoutedEventArgs e)
        {
            _progressSliderBinding = BindingOperations.GetBinding(ProgressSlider, Slider.ValueProperty);

            this.Loaded -= PlayToolbarView_Loaded;

            ProgressSlider.AddHandler(UIElement.PreviewMouseDownEvent,
                            new MouseButtonEventHandler(ProgressSlider_PreviewMouseDown),
                            true);

            ProgressSlider.AddHandler(UIElement.PreviewMouseUpEvent,
                            new MouseButtonEventHandler(ProgressSlider_PreviewMouseUp),
                            true);
        }

        private void AdvancedControlButton_Click(object sender, RoutedEventArgs e)
        {
            double fromHeight = _isAdvancedVisible ? PopupContent.ActualHeight : 0;
            double toHeight = _isAdvancedVisible ? 0 : 50; 

            var heightAnimation = new DoubleAnimation
            {
                From = fromHeight,
                To = toHeight,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            PopupContent.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
            _isAdvancedVisible = !_isAdvancedVisible;
        }

        private void SetSpeedSlider_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetSpeedSlider.Value = 0;
        }

        private void ProgressSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1 || !IsSong()) //hack to avoid issues with double-click
            {
                e.Handled = true;
                return;
            }
            if (sender is Slider slider)
            {
                _progressMouseDowned = true;

                var originalValue = slider.Value;
                BindingOperations.ClearBinding(slider, Slider.ValueProperty);
                slider.Value = originalValue; //This prevents resetting back to 0 when binding cleared.
            }
        }

        private void ProgressSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_progressMouseDowned == false || !IsSong()) //hack to avoid issues with double-click
            {
                e.Handled = true;
                return;
            }
            if (sender is Slider slider)
            {
                _progressMouseDowned = false;
                double newValue = slider.Value;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (DataContext is PlayToolbarViewModel viewModel)
                    {
                        viewModel.TrackTimeChangedCommand.Execute(newValue);
                        BindingOperations.SetBinding(slider, Slider.ValueProperty, _progressSliderBinding);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);                
            }
        }

        private bool IsSong() 
        {
            if (DataContext is PlayToolbarViewModel viewModel && viewModel.IsSong)
            {
                return true;
            }
            return false;
        }
    }
}
