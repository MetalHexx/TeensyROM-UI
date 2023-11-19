using System.IO;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings.Services;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public interface IResetCommand
    {
        Unit Execute();
    }

    public class ResetCommand : TeensyCommand, IResetCommand
    {
        public ResetCommand(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }

        public Unit Execute()
        {
            if (!_serialPort.IsOpen)
            {
                _logService.Log("You must first connect in order to reset the device.");
                return Unit.Default;
            }
            _logService.Log($"Resetting device");

            _serialPort.Write(TeensyConstants.Reset_Bytes.ToArray(), 0, 2);

            return Unit.Default;
        }
    }
}
