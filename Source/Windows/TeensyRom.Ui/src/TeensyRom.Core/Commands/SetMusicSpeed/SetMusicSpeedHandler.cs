using MediatR;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music;

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
            sbyte speed = 0;

            if (request.Type == MusicSpeedCurveTypes.Linear)
            {
                speed = request.Speed.ToSbyte(-68, 128);
                _serialState.SendIntBytes(TeensyToken.SetMusicSpeedLinear, 2);
            }
            else
            {
                speed = request.Speed.ToSbyte(-127, 99);
                _serialState.SendIntBytes(TeensyToken.SetMusicSpeedLog, 2);
            }
            _serialState.SendSignedChar(speed);
            _serialState.HandleAck();
            return Task.FromResult(new SetMusicSpeedResult());
        }
    }
}
