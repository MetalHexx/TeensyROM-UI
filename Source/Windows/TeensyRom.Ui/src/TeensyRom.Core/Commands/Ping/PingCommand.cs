using MediatR;

namespace TeensyRom.Core.Commands.Ping
{
    public class PingCommand : IRequest<PingResult> { }
}
