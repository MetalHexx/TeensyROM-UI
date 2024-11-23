using MediatR;
using TeensyRom.Cli.Core.Common;
using TeensyRom.Cli.Core.Serial;
using TeensyRom.Cli.Core.Serial.State;

namespace TeensyRom.Cli.Core.Commands
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
