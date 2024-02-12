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
using TeensyRom.Ui.Features.Music.SongList;

namespace TeensyRom.Ui.Features.Games.GameList
{
    /// <summary>
    /// Interaction logic for GamesListView.xaml
    /// </summary>
    public partial class GameListView : UserControl
    {
        public GameListView()
        {
            InitializeComponent();
        }

        private void OnListViewDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            ListView? listView = sender as ListView;

            if (TryOpenDirectory(listView)) return;

            TryPlayGame(listView);
        }

        private void OnListViewClicked(object sender, MouseButtonEventArgs e)
        {
            TrySelectGame(sender as ListView);
        }

        private bool TrySelectGame(ListView? listView)
        {
            if (listView?.SelectedItem is GameItem fileItem)
            {
                var viewModel = (GameListViewModel)DataContext;
                viewModel.SelectCommand.Execute(fileItem).Subscribe();
                return true;
            }
            return false;
        }

        private bool TryPlayGame(ListView? listView)
        {
            if (listView?.SelectedItem is GameItem fileItem)
            {
                var viewModel = (GameListViewModel)DataContext;
                viewModel.PlayCommand.Execute(fileItem).Subscribe();
                return true;
            }
            return false;
        }

        private bool TryOpenDirectory(ListView? listView)
        {
            if (listView?.SelectedItem is DirectoryItem directoryItem)
            {
                var viewModel = (GameListViewModel)DataContext;
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

        private void GameListViewView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView != null && listView.SelectedItem != null)
            {
                listView.ScrollIntoView(listView.SelectedItem);
            }
        }
    }
}
