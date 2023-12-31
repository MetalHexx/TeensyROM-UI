using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class SaveFileCommand : IRequest<SaveFileResult>
    {
        public TeensyFileInfo File { get; init; } = default!;
    }
}