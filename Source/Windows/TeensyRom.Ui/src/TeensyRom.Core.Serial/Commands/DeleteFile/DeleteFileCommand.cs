using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Commands.DeleteFile
{
    public class DeleteFileCommand(TeensyStorageType storageType, FilePath path, string? deviceId = null) : ITeensyCommand<DeleteFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public FilePath Path { get; } = path;
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
    }
}
