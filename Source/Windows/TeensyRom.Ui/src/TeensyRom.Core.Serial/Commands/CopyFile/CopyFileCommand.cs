using MediatR;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Commands
{
    public class CopyFileCommand(TeensyStorageType storageType, FilePath sourcePath, FilePath destPath) : IRequest<CopyFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public FilePath SourcePath { get; } = sourcePath;
        public FilePath DestPath { get; } = destPath;
    }
}