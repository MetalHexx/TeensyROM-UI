using MediatR;
using System.Drawing;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Serial.Commands.ToggleMusic
{
    public class ToggleMusicCommand() : IRequest<ToggleMusicResult>;
}
