using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands.Behaviors
{
    public interface ITeensyCommandExecutor
    {
        Task Execute(Func<Task> action, bool queued);
    }

    public class TeensyCommandExecutor(ISerialStateContext serialState, IAlertService alert) : ITeensyCommandExecutor
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ISerialStateContext _serialState = serialState;
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
                _serialState.Lock();
                await action();
                _serialState.ReadSerialAsString(10);
            }
            finally
            {
                _serialState.Unlock();
                _semaphore.Release();
            }
        }
    }
}
