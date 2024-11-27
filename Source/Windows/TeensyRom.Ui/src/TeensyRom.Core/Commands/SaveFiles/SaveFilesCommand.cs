using MediatR;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.SaveFiles
{
    public class SaveFilesCommand(List<FileTransferItem> files) : IRequest<SaveFilesResult>
    {
        public List<FileTransferItem> Files { get; } = files;
    }
}