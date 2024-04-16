using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace TeensyRom.Ui.Features.Common.Models
{
    public class NoCloseDialogViewModel(string title, string content) : ReactiveObject
    {
        [Reactive] public string Title { get; set; } = string.IsNullOrWhiteSpace(title) ? string.Empty : title;
        [Reactive] public string Content { get; set; } = content;
    }
}
