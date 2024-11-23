using MediatR;
using TeensyRom.Cli.Core.Serial.State;

namespace TeensyRom.Cli.Core.Commands.SendString
{
    public class SendStringCommandHandler(ISerialStateContext _serialState) : IRequestHandler<SendStringCommand, SendStringResult>
    {
        public Task<SendStringResult> Handle(SendStringCommand request, CancellationToken cancellationToken)
        {
            _serialState.Write(request.StringToSend);   
            return Task.FromResult(new SendStringResult());
        }
    }
}