using MediatR;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Commands.SetMusicSpeed
{
    public class SetMusicSpeedHandler : IRequestHandler<SetMusicSpeedCommand, SetMusicSpeedResult>
    {
        private readonly ISerialStateContext _serialState;

        public SetMusicSpeedHandler(ISerialStateContext serialState, ISettingsService settings)
        {
            _serialState = serialState;
        }

        public Task<SetMusicSpeedResult> Handle(SetMusicSpeedCommand request, CancellationToken cancellationToken)
        {
            _serialState.SendIntBytes(TeensyToken.SetMusicSpeed, 2);
            _serialState.SendSignedChar(request.Speed.ToSbyte(-127, 127));
            _serialState.HandleAck();
            return Task.FromResult(new SetMusicSpeedResult());
        }
    }
}
