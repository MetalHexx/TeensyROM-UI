using System.Windows.Controls;
using System.Windows;
using TeensyRom.Ui.Features.Music;

namespace TeensyRom.Ui.Features.Common.Models
{
    public class StorageItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DirectoryTemplate { get; set; } = new();
        public DataTemplate FileTemplate { get; set; } = new();
        public DataTemplate SongTemplate { get; set; } = new();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                DirectoryItemVm _ => DirectoryTemplate,
                FileItemVm _ => FileTemplate,
                SongItemVm _ => SongTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}
