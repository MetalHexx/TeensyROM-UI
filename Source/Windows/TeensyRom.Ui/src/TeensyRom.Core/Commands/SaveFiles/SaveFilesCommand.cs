using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class SaveFilesCommand(List<FileTransferItem> files) : IRequest<SaveFilesResult>
    {
        public List<FileTransferItem> Files { get; } = files;
    }
}