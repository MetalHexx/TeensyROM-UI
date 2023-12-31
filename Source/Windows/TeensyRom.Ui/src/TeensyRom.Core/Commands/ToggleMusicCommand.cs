using MediatR;
using System.Drawing;
using System.Reactive;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Commands
{
    public class ToggleMusicCommand() : IRequest<ToggleMusicResponse>;
    public class ToggleMusicResponse() : CommandResult;
    public class ToggleMusicHandler : TeensyCommand, IRequestHandler<ToggleMusicCommand, ToggleMusicResponse>
    {
        public ToggleMusicHandler(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
            : base(settingsService, serialPort, logService) { }

        public Task<ToggleMusicResponse> Handle(ToggleMusicCommand request, CancellationToken cancellationToken)
        {
            _serialPort.SendIntBytes(TeensyConstants.PauseMusicToken, 2);

            if (!GetAck())
            {
                _serialPort.ReadSerialAsString(msToWait: 100);
                throw new TeensyException("Error getting acknowledgement when pause music token sent");
            }
            return Task.FromResult(new ToggleMusicResponse());
        }
    }
}
