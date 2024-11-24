using MediatR;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Cli.Core.Commands
{
    public class PingCommandHandler(ISerialStateContext _serialState) : IRequestHandler<PingCommand, PingResult>
    {
        public Task<PingResult> Handle(PingCommand request, CancellationToken cancellationToken)
        {
            _serialState.Write(TeensyByteToken.Ping_Bytes.ToArray(), 0, 2);

            return Task.FromResult(new PingResult());
        }
    }
}
