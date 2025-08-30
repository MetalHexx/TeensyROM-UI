using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileCommand : ITeensyCommand<LaunchFileResult>
    {
        public LaunchFileCommand(TeensyStorageType storageType, LaunchableItem item, string? deviceId = null)
        {
            StorageType = storageType;
            LaunchItem = item;
            DeviceId = deviceId;
        }
        public FilePath Path => LaunchItem.Path;
        public long Size => LaunchItem.Size;
        public TeensyStorageType StorageType { get; }
        public LaunchableItem LaunchItem { get; }
        public string? DeviceId { get; set; }
        public ISerialStateContext Serial { get; set; } = null!;
    }
}