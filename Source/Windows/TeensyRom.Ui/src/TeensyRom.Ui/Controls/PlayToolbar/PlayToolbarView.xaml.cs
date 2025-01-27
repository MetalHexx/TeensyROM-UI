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

namespace TeensyRom.Ui.Controls.PlayToolbar
{
    /// <summary>
    /// Interaction logic for PlayToolbarView.xaml
    /// </summary>
    public partial class PlayToolbarView : UserControl
    {
        private bool _isAdvancedVisible = false;

        public PlayToolbarView()
        {
            InitializeComponent();
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
    }
}
