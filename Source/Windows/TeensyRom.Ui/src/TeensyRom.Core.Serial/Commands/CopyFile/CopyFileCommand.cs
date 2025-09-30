using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Commands
{
    public class CopyFileCommand(TeensyStorageType storageType, FilePath sourcePath, FilePath destPath, string? deviceId = null) : ITeensyCommand<CopyFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public FilePath SourcePath { get; } = sourcePath;
        public FilePath DestPath { get; } = destPath;
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
    }
}