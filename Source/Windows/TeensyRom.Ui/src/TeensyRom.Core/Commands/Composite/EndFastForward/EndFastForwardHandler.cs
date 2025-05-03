using MediatR;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Commands.SetMusicSpeed;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music;

namespace TeensyRom.Core.Commands.Composite.EndFastForward
{
    public class EndFastForwardHandler(
        IToggleMusicSerialRoutine toggleMusic,
        IMuteSidVoicesSerialRoutine muteVoices,
        ISetMusicSpeedSerialRoutine setMusicSpeed) : IRequestHandler<EndFastForwardCommand, EndFastForwardResult>
    {
        public async Task<EndFastForwardResult> Handle(EndFastForwardCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.ShouldEnableVoices)
                {
                    await muteVoices.Execute(VoiceState.Enabled, VoiceState.Enabled, VoiceState.Enabled);
                }
                await setMusicSpeed.Execute(request.OriginalSpeed, MusicSpeedCurveTypes.Logarithmic);
            }
            catch (TeensyException ex)
            {
                return new EndFastForwardResult
                {
                    IsSuccess = false,
                    Error = ex.Message
                };
            }
            return new EndFastForwardResult();
        }
    }
}
