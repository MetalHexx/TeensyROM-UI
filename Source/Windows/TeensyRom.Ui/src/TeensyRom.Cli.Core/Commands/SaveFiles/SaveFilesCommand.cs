using MediatR;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Cli.Core.Commands
{
    public class SaveFilesCommand(List<FileTransferItem> files) : IRequest<SaveFilesResult>
    {
        public List<FileTransferItem> Files { get; } = files;
    }
}