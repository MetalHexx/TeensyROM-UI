using MediatR;
using TeensyRom.Core.Commands.MuteSidVoices;

namespace TeensyRom.Core.Commands.Composite.FastForward
{
    public class FastForwardCommand : IRequest<FastForwardResult>
    {
        public bool ShouldTogglePlay { get; set; } = false;
        public bool ShouldMuteVoices { get; set; } = false;
        public double Speed { get; set; }
    }
}
