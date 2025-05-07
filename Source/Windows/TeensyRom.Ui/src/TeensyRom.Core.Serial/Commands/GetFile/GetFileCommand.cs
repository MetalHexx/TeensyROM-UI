using MediatR;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands.GetFile
{
    public class GetFileCommand(TeensyStorageType storageType, string filePath, string? deviceId = null) : ITeensyCommand<GetFileResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string FilePath { get; } = filePath;
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
    }
}