using MediatR;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Commands
{
    public class FavoriteFileCommand(TeensyStorageType storageType, string sourcePath, string targetPath) : IRequest<FavoriteFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string SourcePath { get; } = sourcePath;
        public string TargetPath { get; } = targetPath;
    }
}