using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;
using System.Runtime.InteropServices;
using Windows.Media;
using Windows.Media.Playback;

namespace TeensyRom.Ui.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern short GetKeyState(int keyCode);

        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12; // ALT key
        private SystemMediaTransportControls _mediaControls;
        private Windows.Media.Playback.MediaPlayer _mediaPlayer;

        public bool IsKeyPressed(int keyCode)
        {
            return (GetKeyState(keyCode) & 0x8000) != 0;
        }

        public bool IsShiftPressed() => IsKeyPressed(VK_SHIFT);
        public bool IsCtrlPressed() => IsKeyPressed(VK_CONTROL);
        public bool IsAltPressed() => IsKeyPressed(VK_MENU);

        public MainWindow()
        {
            InitializeComponent();

            StateChanged += MainWindowStateChangeRaised;
            Activated += MainWindow_Activated;
            InitializeNavigation();

            this.PreviewKeyDown += GlobalKeyDownHandler;
                       
            InitializeMediaControls();
        }

        private void InitializeNavigation()
        {
            MessageBus.Current.Listen<NavigatedMessage>().Subscribe(message =>
            {
                if (LeftNavButton.IsChecked == true)
                {
                    CloseNav();
                    LeftNavButton.IsChecked = false;
                }
            });
        }

        private void InitializeMediaControls()
        {
            _mediaPlayer?.Dispose();
            _mediaPlayer = new Windows.Media.Playback.MediaPlayer();
            _mediaPlayer.CommandManager.IsEnabled = false;
            _mediaControls = _mediaPlayer.SystemMediaTransportControls;
            _mediaControls.ButtonPressed -= MediaControls_ButtonPressed;
            _mediaControls.IsEnabled = true;
            _mediaControls.IsPlayEnabled = true;
            _mediaControls.IsPauseEnabled = true;
            _mediaControls.IsStopEnabled = true;
            _mediaControls.IsNextEnabled = true;
            _mediaControls.IsPreviousEnabled = true;
            _mediaControls.ButtonPressed += MediaControls_ButtonPressed;
        }

        private void MainWindow_Activated(object? sender, EventArgs e)
        {
            InitializeMediaControls();
        }

        private void MediaControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    Dispatcher.Invoke(() => MessageBus.Current.SendMessage(KeyboardShortcut.PlayPause, MessageBusConstants.MediaPlayerKeyPressed));
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    Dispatcher.Invoke(() => MessageBus.Current.SendMessage(KeyboardShortcut.PlayPause, MessageBusConstants.MediaPlayerKeyPressed));
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    Dispatcher.Invoke(() => MessageBus.Current.SendMessage(KeyboardShortcut.Stop, MessageBusConstants.MediaPlayerKeyPressed));
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Dispatcher.Invoke(() => MessageBus.Current.SendMessage(KeyboardShortcut.NextTrack, MessageBusConstants.MediaPlayerKeyPressed));
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Dispatcher.Invoke(() => MessageBus.Current.SendMessage(KeyboardShortcut.PreviousTrack, MessageBusConstants.MediaPlayerKeyPressed));
                    break;
            }
        }

        private void GlobalKeyDownHandler(object sender, KeyEventArgs e)
        {
            KeyboardShortcut? shortcut = e.Key switch
            {
                Key.Delete => KeyboardShortcut.DisengageFocus,
                Key.Escape => KeyboardShortcut.DisengageFocus,
                Key.D1 => KeyboardShortcut.Voice1Toggle,
                Key.NumPad1 => KeyboardShortcut.Voice1Toggle,
                Key.D2 => KeyboardShortcut.Voice2Toggle,
                Key.NumPad2 => KeyboardShortcut.Voice2Toggle,
                Key.D3 => KeyboardShortcut.Voice3Toggle,
                Key.NumPad3 => KeyboardShortcut.Voice3Toggle,
                Key.Add => KeyboardShortcut.IncreaseSpeedFine,
                Key.Subtract => KeyboardShortcut.DecreaseSpeedFine,
                Key.PageUp when IsCtrlPressed() => KeyboardShortcut.IncreaseSpeed10,
                Key.PageDown when IsCtrlPressed() => KeyboardShortcut.DecreaseSpeed10,
                Key.PageUp => KeyboardShortcut.IncreaseSpeed1,
                Key.PageDown => KeyboardShortcut.DecreaseSpeed1,
                Key.Home => KeyboardShortcut.Restart,
                //Key.Up => KeyboardShortcut.IncreaseSpeed10,
                //Key.Down => KeyboardShortcut.DecreaseSpeed10,
                //Key.Left => KeyboardShortcut.DecreaseSpeed1,
                //Key.Right => KeyboardShortcut.IncreaseSpeed1,
                Key.Multiply => KeyboardShortcut.SetSpeedPlus50,
                Key.Divide => KeyboardShortcut.SetSpeedMinus50,
                Key.NumPad0 => KeyboardShortcut.DefaultSpeed,
                Key.Decimal => KeyboardShortcut.FastForward,
                _ => null
            };

            if (shortcut is null) return;

            var ignoreScrollKeys = (Keyboard.FocusedElement is ScrollViewer
                || Keyboard.FocusedElement is ListBox
                || Keyboard.FocusedElement is DataGrid
                || Keyboard.FocusedElement is ListView
                || Keyboard.FocusedElement is ListViewItem)
                && (shortcut == KeyboardShortcut.IncreaseSpeed1
                    || shortcut == KeyboardShortcut.DecreaseSpeed1
                    || shortcut == KeyboardShortcut.IncreaseSpeed10
                    || shortcut == KeyboardShortcut.DecreaseSpeed10);

            var ignoreTypingKeys = (Keyboard.FocusedElement is TextBox)
                && (e.Key == Key.Delete
                    || shortcut == KeyboardShortcut.Restart
                    || shortcut == KeyboardShortcut.Voice1Toggle
                    || shortcut == KeyboardShortcut.Voice2Toggle
                    || shortcut == KeyboardShortcut.Voice3Toggle
                    || shortcut == KeyboardShortcut.IncreaseSpeedFine
                    || shortcut == KeyboardShortcut.DecreaseSpeedFine);

            if (ignoreScrollKeys || ignoreTypingKeys) return;

            if (shortcut is KeyboardShortcut.DisengageFocus)
            {
                ClearFocus();
                return;
            }

            switch (shortcut)
            {
                case KeyboardShortcut.Voice1Toggle or KeyboardShortcut.Voice2Toggle or KeyboardShortcut.Voice3Toggle:
                    MessageBus.Current.SendMessage(shortcut.Value, MessageBusConstants.SidVoiceMuteKeyPressed);
                    return;
                case KeyboardShortcut.IncreaseSpeedFine:
                    MessageBus.Current.SendMessage(0.1, MessageBusConstants.SidSpeedIncreaseKeyPressed);
                    return;
                case KeyboardShortcut.DecreaseSpeedFine:
                    MessageBus.Current.SendMessage(-0.1, MessageBusConstants.SidSpeedDecreaseKeyPressed);
                    return;
                case KeyboardShortcut.IncreaseSpeed1:
                    MessageBus.Current.SendMessage(1.0, MessageBusConstants.SidSpeedIncreaseKeyPressed);
                    return;
                case KeyboardShortcut.DecreaseSpeed1:
                    MessageBus.Current.SendMessage(-1.0, MessageBusConstants.SidSpeedDecreaseKeyPressed);
                    return;
                case KeyboardShortcut.IncreaseSpeed10:
                    MessageBus.Current.SendMessage(10.0, MessageBusConstants.SidSpeedIncreaseKeyPressed);
                    return;
                case KeyboardShortcut.DecreaseSpeed10:
                    MessageBus.Current.SendMessage(-10.0, MessageBusConstants.SidSpeedDecreaseKeyPressed);
                    return;
                case KeyboardShortcut.SetSpeedPlus50:
                    MessageBus.Current.SendMessage(shortcut.Value, MessageBusConstants.SidSpeedIncrease50KeyPressed);
                    return;
                case KeyboardShortcut.SetSpeedMinus50:
                    MessageBus.Current.SendMessage(shortcut.Value, MessageBusConstants.SidSpeedDecrease50KeyPressed);
                    return;
                case KeyboardShortcut.DefaultSpeed:
                    MessageBus.Current.SendMessage(shortcut.Value, MessageBusConstants.SidSpeedHomeKeyPressed);
                    return;
                case KeyboardShortcut.FastForward:
                    MessageBus.Current.SendMessage(shortcut.Value, MessageBusConstants.FastForwardKeyPressed);
                    return;
                case KeyboardShortcut.Restart:
                    MessageBus.Current.SendMessage(shortcut.Value, MessageBusConstants.RestartKeyPressed);
                    return;
            }
        }

        /// <summary>
        /// Hack to clear the UI of all focusable elements by using a tiny transparent control.
        /// </summary>
        private void ClearFocus()
        {
            if (FocusResetButton != null)
            {
                Keyboard.Focus(FocusResetButton);
            }
        }

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        // State change
        private void MainWindowStateChangeRaised(object? sender, EventArgs e)
        {            
            if (WindowState == WindowState.Maximized)
            {
                MainWindowBorder.BorderThickness = new Thickness(8);
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainWindowBorder.BorderThickness = new Thickness(0);
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }

        private void LeftNavButton_Click(object sender, RoutedEventArgs e)
        {
            if (LeftNavButton.IsChecked == true)
            {
                Overlay.Visibility = Visibility.Visible;
                BeginStoryboard((Storyboard)FindResource("OpenMenu"));
                BeginStoryboard((Storyboard)FindResource("FadeInOverlay"));
                return;
            }
            CloseNav();
        }

        private void CloseNav()
        {
            BeginStoryboard((Storyboard)FindResource("CloseMenu"));
            Storyboard fadeOutStoryboard = (Storyboard)FindResource("FadeOutOverlay");
            BeginStoryboard(fadeOutStoryboard);
            fadeOutStoryboard.Completed += (s, eArgs) =>
            {
                Overlay.Visibility = Visibility.Hidden;
            };
        }

        private void Overlay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LeftNavButton.IsChecked = !LeftNavButton.IsChecked;
            LeftNavButton_Click(LeftNavButton, null!);
        }
    }
}
