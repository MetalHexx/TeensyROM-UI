using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Serial;

namespace TeensyRom.Core.Commands
{
    public class PingCommandHandler : IRequestHandler<PingCommand, PingResult>
    {
        public Task<PingResult> Handle(PingCommand request, CancellationToken cancellationToken)
        {
            request.Serial.SendIntBytes(TeensyToken.Ping, 2);
            var response = request.Serial.ReadAndLogSerialAsString(30);

            return Task.FromResult(new PingResult 
            {
                Response = response,
                IsBusy = response.Contains("busy", StringComparison.OrdinalIgnoreCase)
            });
        }
    }
}
