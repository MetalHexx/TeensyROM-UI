using MediatR;

namespace TeensyRom.Core.Commands.SendString
{
    public record SendStringCommand(string StringToSend) : IRequest<SendStringResult>;
}