using MediatR;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Commands
{
    public class FavoriteFileCommand(TeensyStorageType storageType, FilePath sourcePath, FilePath targetPath) : IRequest<FavoriteFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public FilePath SourcePath { get; } = sourcePath;
        public FilePath TargetPath { get; } = targetPath;
    }
}