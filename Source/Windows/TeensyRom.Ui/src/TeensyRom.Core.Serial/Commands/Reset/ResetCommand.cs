using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Commands
{
    public class ResetCommand : IRequest<ResetResult> { }
}
