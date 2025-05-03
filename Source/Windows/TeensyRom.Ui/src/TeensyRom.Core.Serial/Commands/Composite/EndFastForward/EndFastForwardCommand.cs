using MediatR;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Music;

namespace TeensyRom.Core.Serial.Commands.Composite.EndFastForward
{
    public class EndFastForwardCommand : IRequest<EndFastForwardResult>
    {
        public bool ShouldEnableVoices { get; set; } = false;
        public double OriginalSpeed { get; set; }
        public MusicSpeedCurveTypes SpeedCurve { get; set; }
    }
}
