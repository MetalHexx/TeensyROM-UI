using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands.Behaviors
{
    public interface ITeensyCommandExecutor
    {
        Task Execute(Func<Task> action, bool queued);
    }

    public class TeensyCommandExecutor(IObservableSerialPort serialPort, IAlertService alert) : ITeensyCommandExecutor
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IObservableSerialPort _serialPort = serialPort;
        private readonly IAlertService _alert = alert;

        public async Task Execute(Func<Task> action, bool queued)
        {

            if(_semaphore.CurrentCount == 0)
            {
                if(queued is false) throw new TeensyException("TR is busy with your previous command.  Try again soon.");

                _alert.Publish("Teensy is busy with another operation, your command will be queued.");
            }
            await _semaphore.WaitAsync();

            try
            {
                _serialPort.Lock();
                await action();
                Thread.Sleep(200); //To give TR a chance to be ready to for next command in case UI is spamming commands.
            }
            finally
            {
                _serialPort.Unlock();
                _semaphore.Release();
            }
        }
    }
}
