using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class SaveFilesCommand : IRequest<SaveFilesResult>
    {
        public List<TeensyFileInfo> Files { get; init; } = default!;
    }
}