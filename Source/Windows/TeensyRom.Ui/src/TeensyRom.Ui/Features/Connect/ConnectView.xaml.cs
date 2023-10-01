using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TeensyRom.Ui.Features.Connect
{
    /// <summary>
    /// Interaction logic for ConnectView.xaml
    /// </summary>
    public partial class ConnectView : UserControl
    {
        public ConnectView()
        {
            InitializeComponent();

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(TextBox.TextProperty, typeof(TextBox));
            if (dpd != null)
            {
                dpd.AddValueChanged(LogTextBox, (sender, args) => LogTextBox.ScrollToEnd());
            }
        }
    }
}
