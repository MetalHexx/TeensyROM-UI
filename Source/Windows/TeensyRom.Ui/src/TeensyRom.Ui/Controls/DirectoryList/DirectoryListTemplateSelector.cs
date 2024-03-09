using System.Windows.Controls;
using System.Windows;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Common;

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
                ILaunchableItem _ => LaunchableTemplate,
                FileItem _ => FileTemplate,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
