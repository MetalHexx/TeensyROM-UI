using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryRecursiveCommand(TeensyStorageType storageType, DirectoryPath path, bool recursive, string? deviceId = null) : ITeensyCommand<GetDirectoryRecursiveResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public DirectoryPath Path { get; } = path;
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
        public bool Recursive { get; set; } = recursive;
    }
}
