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
            _serialPort.HandleAck();
            return Task.FromResult(new ToggleMusicResult());
        }
    }
}
