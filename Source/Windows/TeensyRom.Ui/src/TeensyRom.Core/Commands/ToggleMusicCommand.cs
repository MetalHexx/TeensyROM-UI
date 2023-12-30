using MediatR;
using System.Drawing;
using System.Reactive;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Commands
{
    public record ToggleMusicCommand() : IRequest;
    public class ToggleMusicHandler : TeensyCommand, IRequestHandler<ToggleMusicCommand>
    {
        public ToggleMusicHandler(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }

        public Task Handle(ToggleMusicCommand request, CancellationToken cancellationToken)
        {
            _logService.Log("Sending music pause command");

            _serialPort.SendIntBytes(TeensyConstants.PauseMusicToken, 2);

            if (!GetAck())
            {
                ReadSerialAsString(msToWait: 100);
                _logService.Log("Error getting acknowledgement of pause music command");
                _serialPort.EnableAutoReadStream();
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
    }
}
