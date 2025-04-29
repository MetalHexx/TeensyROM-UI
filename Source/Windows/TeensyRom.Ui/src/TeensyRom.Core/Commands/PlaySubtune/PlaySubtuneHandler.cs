using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Commands.PlaySubtune
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

            try
            {
                serialState.HandleAck();
            }
            catch (TeensyException ex)
            {
                return new PlaySubtuneResult
                {
                    IsSuccess = false,
                    Error = ex.Message
                };
            }
            return new PlaySubtuneResult();
        }
    }
}
