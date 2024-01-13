using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Threading;
using TeensyRom.Core.Commands.Behaviors;

namespace TeensyRom.Ui.Services
{
    public class AlertService : IAlertService
    {
        public SnackbarMessageQueue MessageQueue { get; private set; }

        public AlertService(Dispatcher dispatcher, ICommandErrorService commandErrorService)
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
