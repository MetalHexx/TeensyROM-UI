using MediatR;

namespace TeensyRom.Core.Commands
{
    public class ToggleMusicHandler(IToggleMusicSerialRoutine toggleMusic) :  IRequestHandler<ToggleMusicCommand, ToggleMusicResult>
    {
        public Task<ToggleMusicResult> Handle(ToggleMusicCommand request, CancellationToken cancellationToken)
        {
            toggleMusic.Execute();
            return Task.FromResult(new ToggleMusicResult());
        }
    }
}
