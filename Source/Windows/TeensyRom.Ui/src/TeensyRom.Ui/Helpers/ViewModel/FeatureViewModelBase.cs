using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace TeensyRom.Ui.Helpers.ViewModel
{
    public class FeatureViewModelBase: ReactiveObject
    {
        [Reactive] public string FeatureTitle { get; set; } = string.Empty;
    }
}
