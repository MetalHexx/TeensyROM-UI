using MediatR;

namespace TeensyRom.Core.Commands.SendString
{
    public class SendStringCommandHandler : IRequestHandler<SendStringCommand, SendStringResult>
    {
        public Task<SendStringResult> Handle(SendStringCommand request, CancellationToken cancellationToken)
        {
            request.Serial.Write(request.StringToSend);   
            return Task.FromResult(new SendStringResult());
        }
    }
}