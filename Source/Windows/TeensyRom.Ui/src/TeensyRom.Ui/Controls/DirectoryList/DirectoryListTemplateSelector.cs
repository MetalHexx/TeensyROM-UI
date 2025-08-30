using System.Windows.Controls;
using System.Windows;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Ui.Controls.DirectoryList
{
    public class DirectoryListTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DirectoryTemplate { get; set; } = new();
        public DataTemplate FileTemplate { get; set; } = new();
        public DataTemplate SongTemplate { get; set; } = new();
        public DataTemplate LaunchableTemplate { get; set; } = new();
        public DataTemplate GameTemplate { get; set; } = new();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                DirectoryItem _ => DirectoryTemplate,
                LaunchableItem _ => LaunchableTemplate,
                FileItem _ => FileTemplate,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
