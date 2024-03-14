using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace TeensyRom.Ui.Features.Common.Models
{
    public class ConfirmDialogViewModel(string title, string content) : ReactiveObject
    {
        [Reactive] public string Title { get; set; } = string.IsNullOrWhiteSpace(title) ? "Confirm" : title;
        [Reactive] public string Content { get; set; } = content;
    }
}
