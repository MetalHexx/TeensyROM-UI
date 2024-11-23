using MediatR;
using TeensyRom.Ui.Core.Serial.State;

namespace TeensyRom.Ui.Core.Commands.SendString
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