using System.Windows.Controls;
using System.Windows;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Ui.Features.Common.Models
{
    public class StorageItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DirectoryTemplate { get; set; } = new();
        public DataTemplate FileTemplate { get; set; } = new();
        public DataTemplate SongTemplate { get; set; } = new();
        public DataTemplate GameTemplate { get; set; } = new();
        public DataTemplate HexTemplate { get; set; } = new();
        public DataTemplate ImageTemplate { get; set; } = new();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                DirectoryItem _ => DirectoryTemplate,
                SongItem => SongTemplate,
                GameItem _ => GameTemplate,
                HexItem _ => HexTemplate,
                ImageItem _ => ImageTemplate,
                FileItem _ => FileTemplate,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
