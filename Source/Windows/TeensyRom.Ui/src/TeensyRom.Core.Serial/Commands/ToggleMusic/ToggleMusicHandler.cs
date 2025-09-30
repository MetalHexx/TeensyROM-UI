using MediatR;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Serial.Commands.ToggleMusic
{
    public class ToggleMusicHandler : IRequestHandler<ToggleMusicCommand, ToggleMusicResult>
    {
        public Task<ToggleMusicResult> Handle(ToggleMusicCommand request, CancellationToken cancellationToken)
        {
            request.Serial.SendIntBytes(TeensyToken.PauseMusic, 2);
            request.Serial.HandleAck();
            return Task.FromResult(new ToggleMusicResult());
        }
    }
}
