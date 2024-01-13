using MaterialDesignThemes.Wpf;

namespace TeensyRom.Ui.Services
{
    public interface IAlertService
    {
        SnackbarMessageQueue MessageQueue { get; }
        void Enqueue(string message);
    }
}
