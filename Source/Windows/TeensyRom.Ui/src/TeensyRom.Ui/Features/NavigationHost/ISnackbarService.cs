using MaterialDesignThemes.Wpf;

namespace TeensyRom.Ui.Features.NavigationHost
{
    public interface ISnackbarService
    {
        SnackbarMessageQueue MessageQueue { get; }
        void Enqueue(string message);
    }
}
