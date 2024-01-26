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
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TeensyRom.Ui.Features.NavigationHost;

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

            MessageBus.Current.Listen<NavAnimationMessage>().Subscribe(message =>
                {
                    switch (message.NavMenuState)
                    {
                        case NavMenuState.Opened:
                            BeginStoryboard((Storyboard)FindResource("OpenMenu"));
                            break;
                        case NavMenuState.Closed:
                            BeginStoryboard((Storyboard)FindResource("CloseMenu"));
                            break;
                        default:
                            break;
                    }
                });
        }
    }
}
