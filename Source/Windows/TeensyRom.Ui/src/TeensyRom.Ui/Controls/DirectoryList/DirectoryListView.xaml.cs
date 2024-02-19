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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.DirectoryList;
using TeensyRom.Ui.Features.Music.SongList;

namespace TeensyRom.Ui.Controls.DirectoryList
{
    public partial class DirectoryListView : UserControl
    {
        public DirectoryListView()
        {
            InitializeComponent();
        }

        private void OnListViewDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            ListView? listView = sender as ListView;

            if (TryOpenDirectory(listView)) return;

            TryLaunch(listView);
        }

        private void OnListViewClicked(object sender, MouseButtonEventArgs e)
        {
            TrySelect(sender as ListView);
        }

        private bool TrySelect(ListView? listView)
        {
            if (listView?.SelectedItem is ILaunchableItem launchItem)
            {
                var viewModel = (DirectoryListViewModel)DataContext;
                viewModel.SelectCommand.Execute(launchItem).Subscribe();
                return true;
            }
            return false;
        }

        private bool TryLaunch(ListView? listView)
        {
            if (listView?.SelectedItem is ILaunchableItem launchable)
            {
                var viewModel = (DirectoryListViewModel)DataContext;
                viewModel.PlayCommand.Execute(launchable).Subscribe();
                return true;
            }
            return false;
        }

        private bool TryOpenDirectory(ListView? listView)
        {
            if (listView?.SelectedItem is DirectoryItem directoryItem)
            {
                var viewModel = (DirectoryListViewModel)DataContext;
                viewModel.LoadDirectoryCommand.Execute(directoryItem).Subscribe();
                return true;
            }
            return false;
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

        private void DirectoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView != null && listView.SelectedItem != null)
            {
                listView.ScrollIntoView(listView.SelectedItem);
            }
        }
    }
}
