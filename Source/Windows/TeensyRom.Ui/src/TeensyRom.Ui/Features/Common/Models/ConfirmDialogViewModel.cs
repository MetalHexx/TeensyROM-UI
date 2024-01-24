using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace TeensyRom.Ui.Features.Common.Models
{
    public class ConfirmDialogViewModel(string content) : ReactiveObject
    {
        [Reactive] public string Content { get; set; } = content;
    }
}
