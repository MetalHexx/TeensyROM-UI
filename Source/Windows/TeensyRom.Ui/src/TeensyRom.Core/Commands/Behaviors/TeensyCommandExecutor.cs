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
        Task Execute(Func<Task> action);
    }

    public class TeensyCommandExecutor(IObservableSerialPort serialPort) : ITeensyCommandExecutor
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IObservableSerialPort _serialPort = serialPort;

        public async Task Execute(Func<Task> action)
        {
            if(_semaphore.CurrentCount == 0)
            {
                throw new TeensyException("TR is busy with your previous command.  Try again soon.");
            }
            await _semaphore.WaitAsync();

            try
            {
                _serialPort.Lock();
                await action();
            }
            finally
            {
                _serialPort.Unlock();
                _semaphore.Release();
            }
        }
    }
}
