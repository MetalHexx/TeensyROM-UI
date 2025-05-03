using MediatR;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Commands.PlaySubtune;
using TeensyRom.Core.Commands.SetMusicSpeed;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music;
using TeensyRom.Core.Serial.Commands.ToggleMusic;

namespace TeensyRom.Core.Serial.Commands.Composite.StartSeek
{
    public class StartSeekHandler(
        IPlaySubtuneSerialRoutine playSubtune,
        IToggleMusicSerialRoutine toggleMusic,
        IMuteSidVoicesSerialRoutine muteVoices,
        ISetMusicSpeedSerialRoutine setMusicSpeed) : IRequestHandler<StartSeekCommand, StartSeekResult>
    {
        public async Task<StartSeekResult> Handle(StartSeekCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.ShouldTogglePlay)
                {
                    toggleMusic.Execute();
                }
                if (request.Direction is SeekDirection.Backward)
                {
                    playSubtune.Execute((uint)request.SubtuneIndex);
                }
                if (request.ShouldMuteVoices)
                {
                    await muteVoices.Execute(VoiceState.Disabled, VoiceState.Disabled, VoiceState.Disabled);
                }
                await setMusicSpeed.Execute(request.SeekSpeed, MusicSpeedCurveTypes.Logarithmic);
            }
            catch (TeensyException ex)
            {
                return new StartSeekResult
                {
                    IsSuccess = false,
                    Error = ex.Message
                };
            }
            return new StartSeekResult();
        }
    }
}
