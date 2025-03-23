using System.Windows.Controls;
using System.Windows;

namespace TeensyRom.Ui.Features.Settings
{
    public class MidiMappingTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NoteMappingTemplate { get; set; } = null!;
        public DataTemplate DualNoteMappingTemplate { get; set; } = null!;
        public DataTemplate CCMappingTemplate { get; set; } = null!;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                DualNoteMappingViewModel _ => DualNoteMappingTemplate,
                NoteMappingViewModel _ => NoteMappingTemplate,
                CCMappingViewModel _ => CCMappingTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}