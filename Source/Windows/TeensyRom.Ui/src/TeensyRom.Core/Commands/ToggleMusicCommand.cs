using System.Drawing;
using System.Reactive;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Commands
{
    public interface IToggleMusicCommand
    {
        bool Execute();
    }
    public class ToggleMusicCommand : TeensyCommand, IToggleMusicCommand
    {
        public ToggleMusicCommand(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }

        public bool Execute()
        {
            _logService.Log("Sending music pause command");
            _serialPort.DisableAutoReadStream();

            _serialPort.SendIntBytes(TeensyConstants.PauseMusicToken, 2);

            if (!GetAck())
            {                
                ReadSerialAsString(msToWait: 100);
                _logService.Log("Error getting acknowledgement of pause music command");
                _serialPort.EnableAutoReadStream();
                return false;
            }
            _serialPort.EnableAutoReadStream();
            return true;
        }
    }
}
