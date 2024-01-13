using MaterialDesignThemes.Wpf;

namespace TeensyRom.Ui.Services
{
    public interface ISnackbarService
    {
        SnackbarMessageQueue MessageQueue { get; }
        void Enqueue(string message);
    }
}
