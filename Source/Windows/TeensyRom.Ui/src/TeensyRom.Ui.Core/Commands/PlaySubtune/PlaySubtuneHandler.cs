using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Ui.Core.Commands.File.LaunchFile;
using TeensyRom.Ui.Core.Serial.State;
using TeensyRom.Ui.Core.Serial;

namespace TeensyRom.Ui.Core.Commands.PlaySubtune
{
    public class PlaySubtuneCommand (int subtuneIndex) : IRequest<PlaySubtuneResult>
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
