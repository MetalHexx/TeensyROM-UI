using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Commands
{
    public interface IPingCommand
    {
        Unit Execute();
    }
    public class PingCommand : TeensyCommand, IPingCommand
    {
        public PingCommand(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }

        public Unit Execute()
        {
            if (!_serialPort.IsOpen)
            {
                _logService.Log("You must first connect in order to ping the device.");
                return Unit.Default;
            }
            _logService.Log($"Pinging device");

            _serialPort.Write(TeensyConstants.Ping_Bytes.ToArray(), 0, 2);

            return Unit.Default;
        }
    }
}
