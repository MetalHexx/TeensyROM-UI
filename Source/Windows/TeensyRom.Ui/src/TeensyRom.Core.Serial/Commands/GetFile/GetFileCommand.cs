using MediatR;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Commands.GetFile
{
    public class GetFileCommand(TeensyStorageType storageType, string filePath) : IRequest<GetFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string FilePath { get; } = filePath;
    }
}