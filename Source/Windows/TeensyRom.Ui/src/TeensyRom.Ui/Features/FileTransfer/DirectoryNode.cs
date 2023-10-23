using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using TeensyRom.Core.Files;

namespace TeensyRom.Ui.Features.FileTransfer
{
    /// <summary>
    /// Represents a directory node in the view
    /// </summary>
    public class DirectoryNode : NodeBase
    {
        private ObservableCollection<NodeBase> _children;

        public ObservableCollection<NodeBase> Children
        {
            get => _children;
            set => SetProperty(ref _children, value);
        }

        public DirectoryNode()
        {
            _children = new ObservableCollection<NodeBase> { new EmptyNode() };
        }

        /// <summary>
        /// Adds a node to the child collection.  Can be a file or another directory
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(NodeBase child)
        {
            Children.Add(child);
        }
    }
}
