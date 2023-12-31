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
    public class ToggleMusicHandler(IObservableSerialPort _serialPort) :  IRequestHandler<ToggleMusicCommand, ToggleMusicResponse>
    {
        public Task<ToggleMusicResponse> Handle(ToggleMusicCommand request, CancellationToken cancellationToken)
        {
            _serialPort.SendIntBytes(TeensyToken.PauseMusic, 2);

            if (_serialPort.GetAck() != TeensyToken.Ack)
            {
                _serialPort.ReadSerialAsString(msToWait: 100);
                throw new TeensyException("Error getting acknowledgement when pause music token sent");
            }
            return Task.FromResult(new ToggleMusicResponse());
        }
    }
}
