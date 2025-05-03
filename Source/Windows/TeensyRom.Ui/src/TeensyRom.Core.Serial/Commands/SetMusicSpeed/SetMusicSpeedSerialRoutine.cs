using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music;
using System.Diagnostics;

namespace TeensyRom.Core.Commands.SetMusicSpeed
{
    public interface ISetMusicSpeedSerialRoutine
    {
        Task Execute(double speed, MusicSpeedCurveTypes type);
    }

    public class SetMusicSpeedSerialRoutine(ISerialStateContext serialState) : ISetMusicSpeedSerialRoutine
    {
        public async Task Execute(double requestSpeed, MusicSpeedCurveTypes speedCurve)
        {
            var attemptNumber = 1;

            while (attemptNumber <= 5)
            {
                try
                {
                    short computedSpeed = requestSpeed.ToScaledShort();

                    if (speedCurve == MusicSpeedCurveTypes.Linear)
                    {
                        if (requestSpeed < MusicConstants.Linear_Speed_Min || requestSpeed > MusicConstants.Linear_Speed_Max)
                            throw new ArgumentOutOfRangeException(nameof(requestSpeed), $"Speed must be between {MusicConstants.Linear_Speed_Min} and {MusicConstants.Linear_Speed_Max}.");

                        serialState.SendIntBytes(TeensyToken.SetMusicSpeedLinear, 2);
                    }
                    else
                    {
                        if (requestSpeed < MusicConstants.Log_Speed_Min || requestSpeed > MusicConstants.Log_Speed_Max)
                            throw new ArgumentOutOfRangeException(nameof(requestSpeed), $"Speed must be between {MusicConstants.Log_Speed_Min} and {MusicConstants.Log_Speed_Max}.");

                        serialState.SendIntBytes(TeensyToken.SetMusicSpeedLog, 2);
                    }
                    serialState.SendSignedShort(computedSpeed);
                    serialState.HandleAck();
                    break;
                }
                catch (Exception)
                {
                    Debug.WriteLine($"Caught Exception in SetMusicSpeedHandler.  Attempt: {attemptNumber} Delay: {100}");
                    await Task.Delay(100);

                    if (attemptNumber >= 5)
                    {
                        throw new TeensyDjException();
                    }
                    attemptNumber++;
                    serialState.ClearBuffers();
                    continue;
                }
            }
        }
    }
}
