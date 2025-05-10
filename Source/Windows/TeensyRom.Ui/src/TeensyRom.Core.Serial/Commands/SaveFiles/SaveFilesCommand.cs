using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;

namespace TeensyRom.Core.Commands
{
    public class SaveFilesCommand(List<FileTransferItem> files) : ITeensyCommand<SaveFilesResult>
    {
        public List<FileTransferItem> Files { get; } = files;
        public string? DeviceId { get; set; }
        public ISerialStateContext Serial { get; set; } = null!;
    }
}