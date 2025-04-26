using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Threading;
using TeensyRom.Core.Logging;

namespace TeensyRom.Ui.Services
{
    public class SnackbarService : ISnackbarService
    {
        private readonly TimeSpan _deduplicationWindow = TimeSpan.FromSeconds(3);
        private string? _lastMessage;
        private DateTime _lastMessageTime = DateTime.MinValue;

        public SnackbarMessageQueue MessageQueue { get; }

        public SnackbarService(Dispatcher dispatcher, IAlertService commandErrorService)
        {
            MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(3), dispatcher);

            commandErrorService.CommandErrors.Subscribe(Enqueue);
        }

        public void Enqueue(string message)
        {
            var now = DateTime.UtcNow;

            if (message == _lastMessage && (now - _lastMessageTime) < _deduplicationWindow)
            {
                return;
            }
            _lastMessage = message;
            _lastMessageTime = now;

            MessageQueue.Enqueue(message);
        }
    }
}