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


namespace TeensyRom.Ui.Main
{
    public enum KeyboardShortcut
    {
        Voice1Toggle,
        Voice2Toggle,
        Voice3Toggle,
        PlayPause,
        Stop,
        NextTrack,
        PreviousTrack,
        IncreaseSpeed,
        DecreaseSpeed,
        IncreaseSpeed50,
        DecreaseSpeed50,
        DefaultSpeed,
        FastForward,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern short GetKeyState(int keyCode);

        private const int VK_NUMLOCK = 0x90;

        public bool IsNumLockOn()
        {
            return (GetKeyState(VK_NUMLOCK) & 1) == 1;
        }

        public MainWindow()
        {
            InitializeComponent();

            StateChanged += MainWindowStateChangeRaised;

            MessageBus.Current.Listen<NavigatedMessage>().Subscribe(message =>
            {
                if (LeftNavButton.IsChecked == true)
                {
                    CloseNav();                    
                    LeftNavButton.IsChecked = false;
                }
            });

            this.PreviewKeyDown += GlobalKeyDownHandler;

        }

        private void GlobalKeyDownHandler(object sender, KeyEventArgs e)
        {   
            KeyboardShortcut? shortcut = e.Key switch
            {
                Key.D1 => KeyboardShortcut.Voice1Toggle,
                Key.NumPad1 => KeyboardShortcut.Voice1Toggle,
                Key.D2 => KeyboardShortcut.Voice2Toggle,
                Key.NumPad2 => KeyboardShortcut.Voice2Toggle,
                Key.D3 => KeyboardShortcut.Voice3Toggle,
                Key.NumPad3 => KeyboardShortcut.Voice3Toggle,
                Key.Add => KeyboardShortcut.IncreaseSpeed,
                Key.Subtract => KeyboardShortcut.DecreaseSpeed,
                Key.Multiply => KeyboardShortcut.IncreaseSpeed50,
                Key.Divide => KeyboardShortcut.DecreaseSpeed50,    
                Key.NumPad0 => KeyboardShortcut.DefaultSpeed,
                Key.Decimal => KeyboardShortcut.FastForward,
                _ => null
            };

            if (shortcut is null) return;

            var ignore = Keyboard.FocusedElement is TextBox &&
            (
                shortcut == KeyboardShortcut.Voice1Toggle
                ||
                shortcut == KeyboardShortcut.Voice2Toggle
                ||
                shortcut == KeyboardShortcut.Voice3Toggle
                ||
                shortcut == KeyboardShortcut.IncreaseSpeed
                ||
                shortcut == KeyboardShortcut.DecreaseSpeed
                ||
                shortcut == KeyboardShortcut.IncreaseSpeed50                
                ||
                shortcut == KeyboardShortcut.DecreaseSpeed50
                ||
                shortcut == KeyboardShortcut.DefaultSpeed

            );
            if (ignore) return;

            double amt = IsNumLockOn() ? 10 : 1;

            switch (shortcut)
            {
                case KeyboardShortcut.Voice1Toggle or KeyboardShortcut.Voice2Toggle or KeyboardShortcut.Voice3Toggle:
                    MessageBus.Current.SendMessage(shortcut.Value, MessageBusConstants.SidVoiceMuteKeyPressed);
                    return;

                case KeyboardShortcut.IncreaseSpeed:
                    MessageBus.Current.SendMessage(amt, MessageBusConstants.SidSpeedIncreaseKeyPressed);
                    return;

                case KeyboardShortcut.DecreaseSpeed:
                    MessageBus.Current.SendMessage(amt, MessageBusConstants.SidSpeedDecreaseKeyPressed);
                    return;

                case KeyboardShortcut.IncreaseSpeed50:
                    MessageBus.Current.SendMessage(shortcut.Value, MessageBusConstants.SidSpeedIncrease50KeyPressed);
                    return;

                case KeyboardShortcut.DecreaseSpeed50:
                    MessageBus.Current.SendMessage(shortcut.Value, MessageBusConstants.SidSpeedDecrease50KeyPressed);
                    return;
                case KeyboardShortcut.DefaultSpeed:
                    MessageBus.Current.SendMessage(shortcut.Value, MessageBusConstants.SidSpeedDefaultKeyPressed);
                    return;

                case KeyboardShortcut.FastForward:
                    MessageBus.Current.SendMessage(shortcut.Value, MessageBusConstants.FastForwardKeyPressed);
                    return;
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
