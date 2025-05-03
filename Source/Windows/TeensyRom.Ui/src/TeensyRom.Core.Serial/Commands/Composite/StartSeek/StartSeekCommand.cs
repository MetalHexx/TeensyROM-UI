using MediatR;
using TeensyRom.Core.Commands.MuteSidVoices;

namespace TeensyRom.Core.Serial.Commands.Composite.StartSeek
{
    public class StartSeekCommand : IRequest<StartSeekResult>
    {
        public int SubtuneIndex { get; set; }
        public bool ShouldTogglePlay { get; set; } = false;
        public bool ShouldMuteVoices { get; set; } = false;

        public double SeekSpeed { get; set; }
        public SeekDirection Direction { get; set; } = SeekDirection.Forward;
    }
}
