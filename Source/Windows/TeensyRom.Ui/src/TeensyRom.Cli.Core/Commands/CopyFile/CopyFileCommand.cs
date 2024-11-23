using MediatR;
using TeensyRom.Cli.Core.Storage.Entities;

namespace TeensyRom.Cli.Core.Commands
{
    public class CopyFileCommand(TeensyStorageType storageType, string sourcePath, string destPath) : IRequest<CopyFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string SourcePath { get; } = sourcePath;
        public string DestPath { get; } = destPath;
    }
}