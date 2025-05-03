using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands
{
    public class PingCommandHandler(ISerialStateContext serialState) : IRequestHandler<PingCommand, PingResult>
    {
        public Task<PingResult> Handle(PingCommand request, CancellationToken cancellationToken)
        {
            serialState.SendIntBytes(TeensyToken.Ping, 2);
            return Task.FromResult(new PingResult());
        }
    }
}
