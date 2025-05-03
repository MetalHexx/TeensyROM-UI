using MediatR;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Commands.DeleteFile
{
    public class DeleteFileCommand(TeensyStorageType storageType, string path) : IRequest<DeleteFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string Path { get; } = path;
    }
}
