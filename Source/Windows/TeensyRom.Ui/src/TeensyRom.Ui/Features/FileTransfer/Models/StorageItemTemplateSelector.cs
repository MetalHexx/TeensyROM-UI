using System.Windows.Controls;
using System.Windows;
using TeensyRom.Ui.Features.Common.Models;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Features.FileTransfer.Models
{
    public class StorageItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DirectoryTemplate { get; set; } = new();
        public DataTemplate FileTemplate { get; set; } = new();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                DirectoryItem _ => DirectoryTemplate,
                FileItem _ => FileTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}
