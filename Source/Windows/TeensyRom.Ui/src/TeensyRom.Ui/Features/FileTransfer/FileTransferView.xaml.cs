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

namespace TeensyRom.Ui.Features.FileTransfer
{
    /// <summary>
    /// Interaction logic for FileTransferView.xaml
    /// </summary>
    public partial class FileTransferView : UserControl
    {
        public FileTransferView()
        {
            InitializeComponent();

            //var dpd = DependencyPropertyDescriptor.FromProperty(TextBox.TextProperty, typeof(TextBox));
            //if (dpd != null)
            //{
            //    dpd.AddValueChanged(LogTextBox, (sender, args) => LogTextBox.ScrollToEnd());
            //}
        }

        private void OnTreeViewPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = MouseWheelEvent,
                    Source = sender
                };

                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }
    }
}
