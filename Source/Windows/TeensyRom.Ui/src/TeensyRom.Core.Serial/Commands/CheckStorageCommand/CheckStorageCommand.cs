using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Serial.Commands.CheckStorageCommand
{
    public class CheckStorageCommand(TeensyStorageType storageType, string filePath, string? deviceId = null) : ITeensyCommand<CheckStorageResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string FilePath { get; } = filePath;
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
    }
}