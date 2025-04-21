using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TeensyRom.Core.Music.Midi;
using TeensyRom.Ui.Main;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Controls.PlayToolbar
{
    /// <summary>
    /// Interaction logic for PlayToolbarView.xaml
    /// </summary>
    public partial class PlayToolbarView : UserControl
    {
        private Binding? _progressSliderBinding;
        private bool _progressMouseDowned = false;
        private PlayToolbarViewModel _vm;
        private List<IDisposable> _subscriptions = [];

        public PlayToolbarView()
        {
            InitializeComponent();
            Loaded += PlayToolbarView_Loaded;            
            DataContextChanged += OnDataContextChanged;
            Unloaded += (s, e) => Dispose();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is PlayToolbarViewModel vm)
            {
                _vm = vm;

                _subscriptions.Add
                (
                    _vm.WhenAnyValue(x => x.IsSong)
                    .Subscribe(_ => ToggleAdvancedSeparator())
                );

                _subscriptions.Add
                (
                    _vm.WhenAnyValue(x => x.ProgressEnabled)
                        .Subscribe(_ => ToggleAdvancedSeparator())
                );

                _subscriptions.Add
                (
                    MessageBus.Current.Listen<MidiEvent>(MessageBusConstants.MidiCommandsReceived)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(_ => ToggleOpenAdvancedMenu())
                );

                _subscriptions.Add
                (
                    MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.SidVoiceMuteKeyPressed)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Where(_ => _vm.AdvancedEnabled is false)
                        .Subscribe(_ => ToggleOpenAdvancedMenu())
                );

                _subscriptions.Add
                (
                    MessageBus.Current.Listen<double>(MessageBusConstants.SidSpeedIncreaseKeyPressed)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Where(_ => _vm.AdvancedEnabled is false)
                        .Subscribe(_ => ToggleOpenAdvancedMenu())
                );

                _subscriptions.Add
                (
                    MessageBus.Current.Listen<double>(MessageBusConstants.SidSpeedDecreaseKeyPressed)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Where(_ => _vm.AdvancedEnabled is false)
                        .Subscribe(_ => ToggleOpenAdvancedMenu())
                );

                _subscriptions.Add
                (
                    MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.SidSpeedIncrease50KeyPressed)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Where(_ => _vm.AdvancedEnabled is false)
                        .Subscribe(_ => ToggleOpenAdvancedMenu())
                );

                _subscriptions.Add
                (
                    MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.SidSpeedDecrease50KeyPressed)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Where(_ => _vm.AdvancedEnabled is false)
                        .Subscribe(_ => ToggleOpenAdvancedMenu())
                );

                _subscriptions.Add
                (
                    MessageBus.Current.Listen<KeyboardShortcut>(MessageBusConstants.SidSpeedHomeKeyPressed)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Where(_ => _vm.AdvancedEnabled is false)
                        .Subscribe(_ => ToggleOpenAdvancedMenu())
                );
                if (_vm.AdvancedEnabled) 
                {
                    _vm.AdvancedEnabled = false;
                    AdvancedControlButton_Click(this, null);
                }
            }
        }

        private void ProgressContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!_vm.IsSong || !_vm.ProgressEnabled) 
            {
                return;
            }
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));

            ProgressIndicator.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            ProgressSlider.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private void ProgressContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_vm.IsSong || !_vm.ProgressEnabled)
            {
                return;
            }
            if (!_progressMouseDowned)
            {
                var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
                var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));

                ProgressSlider.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                ProgressIndicator.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
        }

        private void ToggleOpenAdvancedMenu()
        {
            if (!_vm.AdvancedEnabled) 
            {   
                AdvancedControlButton_Click(this, null);
            }            
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

        private void AdvancedControlButton_Click(object sender, RoutedEventArgs? e)
        {
            double fromHeight = _vm.AdvancedEnabled ? PopupContent.ActualHeight : 0;
            double toHeight = _vm.AdvancedEnabled ? 0 : 50;

            var heightAnimation = new DoubleAnimation
            {
                From = fromHeight,
                To = toHeight,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            PopupContent.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);

            _vm.AdvancedEnabled = !_vm.AdvancedEnabled;

            ToggleAdvancedSeparator();
        }

        private void SetSpeedSlider_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetSpeedSlider.Value = 0;            
        }

        private void ProgressSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1 || !_vm.IsSong) //hack to avoid issues with double-click
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
            if (_progressMouseDowned == false || !_vm.IsSong) //hack to avoid issues with double-click
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

        private void ToggleAdvancedSeparator() 
        {
            var shouldEnableSeparator = _vm.AdvancedEnabled && ((_vm.IsSong && !_vm.ProgressEnabled) || (_vm.IsSong is false && !_vm.TimedPlayEnabled));

            AdvancedSeparator.Visibility = shouldEnableSeparator ? Visibility.Visible : Visibility.Hidden;
        }

        public void Dispose()
        {
            _subscriptions.ForEach(s => s?.Dispose());
        }

        private void SetSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _vm.SetSpeed(SetSpeedSlider.Value);
        }
    }
}
