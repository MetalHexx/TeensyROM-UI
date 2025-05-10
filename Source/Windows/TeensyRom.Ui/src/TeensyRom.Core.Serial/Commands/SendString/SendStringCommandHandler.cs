using MediatR;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Commands.SendString
{
    public class SendStringCommandHandler(ISerialStateContext serialState) : IRequestHandler<SendStringCommand, SendStringResult>
    {
        public Task<SendStringResult> Handle(SendStringCommand request, CancellationToken cancellationToken)
        {
            serialState.Write(request.StringToSend);   
            return Task.FromResult(new SendStringResult());
        }
    }
}