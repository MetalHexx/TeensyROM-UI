using MediatR;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Commands.DeleteFile
{
    public class DeleteFileCommand(TeensyStorageType storageType, FilePath path) : IRequest<DeleteFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public FilePath Path { get; } = path;
    }
}
