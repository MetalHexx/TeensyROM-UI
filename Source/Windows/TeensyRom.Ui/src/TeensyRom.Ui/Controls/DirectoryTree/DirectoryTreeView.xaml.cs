using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TeensyRom.Ui.Controls.DirectoryTree
{
    /// <summary>
    /// Interaction logic for DirectoryTreeView.xaml
    /// </summary>
    public partial class DirectoryTreeView : UserControl
    {
        private bool _initialized = false;
        public DirectoryTreeView()
        {
            InitializeComponent();
            Unloaded += DirectoryTreeView_Unloaded;            
            DirectoryTreeControl.MouseLeftButtonUp += TreeView_MouseLeftButtonUp;
        }

        private void TreeView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var treeView = sender as TreeView;

            if (treeView?.SelectedItem is DirectoryNodeViewModel directoryNode)
            {
                var viewModel = (DirectoryTreeViewModel)DataContext;
                viewModel.DirectorySelectedCommand!.Execute(directoryNode);
            }
        }

        private void DirectoryTreeView_Unloaded(object sender, RoutedEventArgs e)
        {
            DirectoryTreeControl.MouseLeftButtonUp -= TreeView_MouseLeftButtonUp;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) 
        {
            if (!_initialized) //skips initial event caused by data load.  We only want the users explicit selection events.
            {
                _initialized = true;
                return;
            }

            var treeView = sender as TreeView;

            if (treeView?.SelectedItem is DirectoryNodeViewModel directoryNode)
            {
                var viewModel = (DirectoryTreeViewModel)DataContext;
                viewModel.DirectorySelectedCommand!.Execute(directoryNode);
            }
        } 
    }
}