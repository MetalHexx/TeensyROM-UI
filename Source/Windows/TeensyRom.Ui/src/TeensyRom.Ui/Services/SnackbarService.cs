using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Threading;
using TeensyRom.Ui.Core.Logging;

namespace TeensyRom.Ui.Services
{
    public class SnackbarService : ISnackbarService
    {
        public SnackbarMessageQueue MessageQueue { get; private set; }

        public SnackbarService(Dispatcher dispatcher, IAlertService commandErrorService)
        {
            MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3), dispatcher);

            commandErrorService.CommandErrors.Subscribe(error =>
            {
                MessageQueue.Enqueue(error);
            });
        }

        public void Enqueue(string message)
        {
            MessageQueue.Enqueue(message);
        }
    }
}
