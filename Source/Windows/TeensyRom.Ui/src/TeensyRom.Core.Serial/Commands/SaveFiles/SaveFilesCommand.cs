using MediatR;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Commands
{
    public class SaveFilesCommand(List<FileTransferItem> files) : IRequest<SaveFilesResult>
    {
        public List<FileTransferItem> Files { get; } = files;
    }
}