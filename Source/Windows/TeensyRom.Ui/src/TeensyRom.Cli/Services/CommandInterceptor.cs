using Spectre.Console;
using Spectre.Console.Cli;
using System.Reactive.Linq;
using TeensyRom.Cli.Helpers;
using TeensyRom.Cli.Core.Common;
using TeensyRom.Cli.Core.Serial.State;

namespace TeensyRom.Cli.Services
{
    internal class CommandInterceptor(ISerialStateContext serial) : ICommandInterceptor
    {
        public void Intercept(CommandContext context, CommandSettings settings)
        {
            if (settings is IRequiresConnection)
            {
                var serialState = serial.CurrentState.FirstAsync().GetAwaiter().GetResult();

                if (serialState is SerialConnectedState)
                {
                    return;
                }
                if (serialState is SerialBusyState)
                {
                    throw new TeensyStateException("Command unavailable while serial is busy!");
                }
                if (serialState is SerialStartState)
                {
                    throw new TeensyStateException("Command unavailable as no connectable ports have been found. ");
                }

                if (serialState is SerialConnectionLostState)
                {
                    throw new TeensyStateException("Command unavailable while trying to re-establish a connection.");
                }
                if (serialState is SerialConnectableState)
                {
                    serial.OpenPort();
                    return;
                }
                throw new TeensyStateException("Command unavailable while serial is in an unknown state.");
            }
        }
        public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
        {
            if (settings is IClearableSettings s)
            {
                s.ClearSettings();
            }
        }
    }
}
