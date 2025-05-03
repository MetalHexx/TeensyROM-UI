using MediatR;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Commands.PlaySubtune;
using TeensyRom.Core.Commands.SetMusicSpeed;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music;

namespace TeensyRom.Core.Commands.Composite.EndSeek
{
    public class EndSeekHandler(
        IMuteSidVoicesSerialRoutine muteVoices,
        ISetMusicSpeedSerialRoutine setMusicSpeed) : IRequestHandler<EndSeekCommand, EndSeekResult>
    {
        public async Task<EndSeekResult> Handle(EndSeekCommand request, CancellationToken cancellationToken)
        {
            try
            {                
                await setMusicSpeed.Execute(request.SeekSpeed, request.SpeedCurve);

                if (request.ShouldEnableVoices)
                {
                    await muteVoices.Execute(VoiceState.Enabled, VoiceState.Enabled, VoiceState.Enabled);
                }
            }
            catch (TeensyException ex)
            {
                return new EndSeekResult
                {
                    IsSuccess = false,
                    Error = ex.Message
                };
            }
            return new EndSeekResult();
        }
    }
}
