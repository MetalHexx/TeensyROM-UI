using MediatR;
using System.IO;
using System.IO.Ports;
using System.Runtime;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryCommand(TeensyStorageType storageType, string path, string? deviceId = null) : ITeensyCommand<GetDirectoryResult>
    {
        public TeensyStorageType StorageType { get; } = storageType;
        public string Path { get; } = path;
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
    }
}
