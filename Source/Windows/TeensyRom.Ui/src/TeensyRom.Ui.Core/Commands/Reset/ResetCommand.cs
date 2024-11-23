using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Ui.Core.Logging;
using TeensyRom.Ui.Core.Settings;
using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Commands
{
    public class ResetCommand : IRequest<ResetResult> { }
}
