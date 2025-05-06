using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileCommand : ITeensyCommand<LaunchFileResult>
    {
        public LaunchFileCommand(TeensyStorageType storageType, ILaunchableItem item, string? deviceId = null)
        {
            StorageType = storageType;
            LaunchItem = item;
            DeviceId = deviceId;
        }
        public string Path => LaunchItem.Path;
        public long Size => LaunchItem.Size;
        public TeensyStorageType StorageType { get; }
        public ILaunchableItem LaunchItem { get; }
        public string? DeviceId { get; set; }
        public ISerialStateContext Serial { get; set; } = null!;
    }
}