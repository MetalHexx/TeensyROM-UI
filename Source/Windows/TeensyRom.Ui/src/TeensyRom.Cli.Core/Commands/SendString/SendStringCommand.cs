using MediatR;

namespace TeensyRom.Cli.Core.Commands.SendString
{
    public record SendStringCommand(string StringToSend) : IRequest<SendStringResult>;
}