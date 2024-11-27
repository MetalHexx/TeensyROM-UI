using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Ui.Core.Commands
{
    public class ToggleMusicHandler(ISerialStateContext _serialState) :  IRequestHandler<ToggleMusicCommand, ToggleMusicResult>
    {
        public Task<ToggleMusicResult> Handle(ToggleMusicCommand request, CancellationToken cancellationToken)
        {
            _serialState.SendIntBytes(TeensyToken.PauseMusic, 2);
            _serialState.HandleAck();
            return Task.FromResult(new ToggleMusicResult());
        }
    }
}
