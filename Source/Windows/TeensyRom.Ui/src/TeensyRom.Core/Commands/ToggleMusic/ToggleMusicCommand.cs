using MediatR;

namespace TeensyRom.Core.Commands.ToggleMusic
{
    public class ToggleMusicCommand() : IRequest<ToggleMusicResult>;
}
