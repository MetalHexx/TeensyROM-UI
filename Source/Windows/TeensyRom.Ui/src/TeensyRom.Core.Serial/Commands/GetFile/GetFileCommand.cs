using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Commands.GetFile
{
    public class GetFileCommand(TeensyStorageType storageType, FilePath filePath, string? deviceId = null) : ITeensyCommand<GetFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public FilePath FilePath { get; } = filePath;
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
    }
}