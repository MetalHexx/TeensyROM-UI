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
            short speed = 0;

            if (request.Type == MusicSpeedCurveTypes.Linear)
            {
                if (speed < MusicConstants.Linear_Speed_Min || speed > MusicConstants.Linear_Speed_Max)
                    throw new ArgumentOutOfRangeException(nameof(speed), $"Speed must be between {MusicConstants.Linear_Speed_Min} and {MusicConstants.Linear_Speed_Max}.");

                speed = request.Speed.ToScaledShort();
                _serialState.SendIntBytes(TeensyToken.SetMusicSpeedLinear, 2);
            }
            else
            {
                if (speed < MusicConstants.Log_Speed_Min || speed > MusicConstants.Log_Speed_Max)
                    throw new ArgumentOutOfRangeException(nameof(speed), $"Speed must be between {MusicConstants.Log_Speed_Min} and {MusicConstants.Log_Speed_Max}.");
                speed = request.Speed.ToScaledShort();
                _serialState.SendIntBytes(TeensyToken.SetMusicSpeedLog, 2);
            }
            _serialState.SendSignedShort(speed);
            _serialState.HandleAck();
            return Task.FromResult(new SetMusicSpeedResult());
        }
    }
}
