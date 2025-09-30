using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Commands
{
    public class FavoriteFileCommand(TeensyStorageType storageType, FilePath sourcePath, FilePath targetPath, string? deviceId = null) : ITeensyCommand<FavoriteFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public FilePath SourcePath { get; } = sourcePath;
        public FilePath TargetPath { get; } = targetPath;
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
    }
}