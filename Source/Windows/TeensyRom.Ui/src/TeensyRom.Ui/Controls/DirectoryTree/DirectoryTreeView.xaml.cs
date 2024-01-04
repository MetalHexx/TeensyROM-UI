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
using TeensyRom.Ui.Features.FileTransfer;

namespace TeensyRom.Ui.Controls.DirectoryTree
{
    /// <summary>
    /// Interaction logic for DirectoryTreeView.xaml
    /// </summary>
    public partial class DirectoryTreeView : UserControl
    {
        public DirectoryTreeView()
        {
            InitializeComponent();
            Unloaded += DirectoryTreeView_Unloaded;
            DirectoryTreeControl.SelectedItemChanged += TreeView_SelectedItemChanged;
        }

        private void DirectoryTreeView_Unloaded(object sender, RoutedEventArgs e)
        {
            DirectoryTreeControl.SelectedItemChanged -= TreeView_SelectedItemChanged;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) 
        {
            var treeView = sender as TreeView;

            if (treeView?.SelectedItem is DirectoryNodeViewModel directoryNode)
            {
                var viewModel = (DirectoryTreeViewModel)DataContext;
                viewModel.DirectorySelectedCommand.Execute(directoryNode);
            }
        } 
    }
}