using MediatR;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands
{
    public class PingCommandHandler(ISerialStateContext _serialState) : IRequestHandler<PingCommand, PingResult>
    {
        public Task<PingResult> Handle(PingCommand request, CancellationToken cancellationToken)
        {
            _serialState.SendIntBytes(TeensyToken.Ping, 2);
            return Task.FromResult(new PingResult());
        }
    }
}
