using MediatR;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Cli.Core.Commands.GetFile
{
    public class GetFileCommand(TeensyStorageType storageType, string filePath) : IRequest<GetFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string FilePath { get; } = filePath;
    }
}