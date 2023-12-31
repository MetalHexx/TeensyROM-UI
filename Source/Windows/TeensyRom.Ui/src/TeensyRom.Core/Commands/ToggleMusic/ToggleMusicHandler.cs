using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands
{
    public class ToggleMusicHandler(IObservableSerialPort _serialPort) :  IRequestHandler<ToggleMusicCommand, ToggleMusicResult>
    {
        public Task<ToggleMusicResult> Handle(ToggleMusicCommand request, CancellationToken cancellationToken)
        {
            _serialPort.SendIntBytes(TeensyToken.PauseMusic, 2);

            if (_serialPort.GetAck() != TeensyToken.Ack)
            {
                _serialPort.ReadSerialAsString(msToWait: 100);
                throw new TeensyException("Error getting acknowledgement when pause music token sent");
            }
            return Task.FromResult(new ToggleMusicResult());
        }
    }
}
