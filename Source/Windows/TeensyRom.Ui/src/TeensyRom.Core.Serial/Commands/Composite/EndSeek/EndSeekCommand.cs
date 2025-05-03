using MediatR;
using TeensyRom.Core.Commands.MuteSidVoices;
using TeensyRom.Core.Music;

namespace TeensyRom.Core.Serial.Commands.Composite.EndSeek
{
    public class EndSeekCommand : IRequest<EndSeekResult>
    {
        public bool ShouldEnableVoices { get; set; } = false;
        public double SeekSpeed { get; set; }
        public MusicSpeedCurveTypes SpeedCurve { get; set; }
    }
}
