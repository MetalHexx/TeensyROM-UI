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

namespace TeensyRom.Ui.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            if (Keyboard.FocusedElement is TextBox)
            {
                return;
            }
            if (e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3)
            {
                int intKey = int.Parse(e.Key.ToString().Replace("D", ""));
                MessageBus.Current.SendMessage(intKey, MessageBusConstants.GlobalVoiceKeyMessageName);
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
