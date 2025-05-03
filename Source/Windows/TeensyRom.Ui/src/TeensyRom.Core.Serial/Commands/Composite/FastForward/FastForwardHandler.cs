using MediatR;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Commands.SetMusicSpeed;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music;
using TeensyRom.Core.Serial.Commands.ToggleMusic;

namespace TeensyRom.Core.Serial.Commands.Composite.FastForward
{
    public class FastForwardHandler(
        IToggleMusicSerialRoutine toggleMusic,
        IMuteSidVoicesSerialRoutine muteVoices,
        ISetMusicSpeedSerialRoutine setMusicSpeed) : IRequestHandler<FastForwardCommand, FastForwardResult>
    {
        public async Task<FastForwardResult> Handle(FastForwardCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.ShouldTogglePlay)
                {
                    toggleMusic.Execute();
                }
                if (request.ShouldMuteVoices)
                {
                    await muteVoices.Execute(VoiceState.Disabled, VoiceState.Disabled, VoiceState.Disabled);
                }
                await setMusicSpeed.Execute(request.Speed, MusicSpeedCurveTypes.Logarithmic);
            }
            catch (TeensyException ex)
            {
                return new FastForwardResult
                {
                    IsSuccess = false,
                    Error = ex.Message
                };
            }
            return new FastForwardResult();
        }
    }
}
