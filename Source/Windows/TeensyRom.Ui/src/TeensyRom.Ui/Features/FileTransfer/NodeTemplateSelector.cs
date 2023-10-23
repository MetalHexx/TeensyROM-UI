using System.Windows.Controls;
using System.Windows;

namespace TeensyRom.Ui.Features.FileTransfer
{
    /// <summary>
    /// Assists the view with rendering the file or directory node templates
    /// </summary>
    public class NodeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DirectoryTemplate { get; set; } = new();
        public DataTemplate FileTemplate { get; set; } = new();
        public DataTemplate EmptyTemplate { get; set; } = new();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                DirectoryNode _ => DirectoryTemplate,
                FileNode _ => FileTemplate,
                EmptyNode _ => EmptyTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}
