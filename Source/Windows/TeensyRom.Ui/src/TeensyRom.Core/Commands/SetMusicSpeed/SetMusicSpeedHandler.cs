using MediatR;

namespace TeensyRom.Core.Commands.SetMusicSpeed
{
    public class SetMusicSpeedHandler(ISetMusicSpeedSerialRoutine setMusicSpeed) : IRequestHandler<SetMusicSpeedCommand, SetMusicSpeedResult>
    {
        public async Task<SetMusicSpeedResult> Handle(SetMusicSpeedCommand request, CancellationToken cancellationToken)
        {
            await setMusicSpeed.Execute(request.Speed, request.Type);
            return new SetMusicSpeedResult();
        }
    }
}
