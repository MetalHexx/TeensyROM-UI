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

        public async Task<SetMusicSpeedResult> Handle(SetMusicSpeedCommand request, CancellationToken cancellationToken)
        {
            var attemptNumber = 1;

            while (attemptNumber <= 3) 
            {
                try
                {
                    short speed = request.Speed.ToScaledShort();

                    if (request.Type == MusicSpeedCurveTypes.Linear)
                    {
                        if (request.Speed < MusicConstants.Linear_Speed_Min || request.Speed > MusicConstants.Linear_Speed_Max)
                            throw new ArgumentOutOfRangeException(nameof(speed), $"Speed must be between {MusicConstants.Linear_Speed_Min} and {MusicConstants.Linear_Speed_Max}.");

                        _serialState.SendIntBytes(TeensyToken.SetMusicSpeedLinear, 2);
                    }
                    else
                    {
                        if (request.Speed < MusicConstants.Log_Speed_Min || request.Speed > MusicConstants.Log_Speed_Max)
                            throw new ArgumentOutOfRangeException(nameof(speed), $"Speed must be between {MusicConstants.Log_Speed_Min} and {MusicConstants.Log_Speed_Max}.");

                        _serialState.SendIntBytes(TeensyToken.SetMusicSpeedLog, 2);
                    }
                    _serialState.SendSignedShort(speed);
                    _serialState.HandleAck();
                    break;
                }
                catch (TeensyException)
                {
                    await Task.Delay(attemptNumber * 100);

                    if (attemptNumber == 3) 
                    {
                        throw new TeensyDjException();
                    }
                    attemptNumber++;
                    continue;
                }
            }
            return new SetMusicSpeedResult();
        }
    }
}
