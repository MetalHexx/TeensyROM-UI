using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Threading;

namespace TeensyRom.Ui.Features.NavigationHost
{
    public class SnackbarService : ISnackbarService
    {
        public SnackbarMessageQueue MessageQueue { get; private set; }

        public SnackbarService(Dispatcher dispatcher)
        {
            MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3), dispatcher);
        }

        public void Enqueue(string message)
        {
            MessageQueue.Enqueue(message);
        }
    }
}
