using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TeensyRom.Ui.Helpers.Messages;

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

            MessageBus.Current.Listen<ScrollToTopMessage>()
                  .ObserveOn(RxApp.MainThreadScheduler)
                  .Subscribe(_ => TargetScrollViewer.ScrollToTop());
        }

        private void OnListViewDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var listView = sender as ListView;

            if (listView?.SelectedItem is DirectoryItemVm directoryItem)
            {
                var viewModel = (FileTransferViewModel)DataContext;
                viewModel.LoadDirectoryContentCommand.Execute(directoryItem).Subscribe();
            }
        }

        private void OnListViewPreviewMouseWheel(object sender, MouseWheelEventArgs e)
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
