using MediatR;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands.PlaySubtune
{
    public class PlaySubtuneCommand(int subtuneIndex) : IRequest<PlaySubtuneResult>
    {
        public int SubtuneIndex { get; } = subtuneIndex;
    }

    public class PlaySubtuneResult : TeensyCommandResult;

    public class PlaySubtuneHandler(ISerialStateContext serialState) : IRequestHandler<PlaySubtuneCommand, PlaySubtuneResult>
    {
        public async Task<PlaySubtuneResult> Handle(PlaySubtuneCommand request, CancellationToken cancellationToken)
        {
            serialState.ClearBuffers();
            serialState.SendIntBytes(TeensyToken.PlaySubtune, 2);
            serialState.SendIntBytes((uint)request.SubtuneIndex - 1, 1);
            serialState.HandleAck();
            return new PlaySubtuneResult();
        }
    }
}
