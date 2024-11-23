using MediatR;
using System.Drawing;
using System.Reactive;
using TeensyRom.Ui.Core.Logging;
using TeensyRom.Ui.Core.Settings;

namespace TeensyRom.Ui.Core.Commands
{
    public class ToggleMusicCommand() : IRequest<ToggleMusicResult>;
}
