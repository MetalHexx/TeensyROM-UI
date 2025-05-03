using MediatR;

namespace TeensyRom.Core.Commands.PlaySubtune
{
    public class PlaySubtuneCommand (int subtuneIndex) : IRequest<PlaySubtuneResult>
    {
        public int SubtuneIndex { get; } = subtuneIndex;
    }
}
