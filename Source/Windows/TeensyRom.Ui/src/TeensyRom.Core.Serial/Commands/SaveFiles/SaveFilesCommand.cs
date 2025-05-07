using MediatR;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands
{
    public class SaveFilesCommand(List<FileTransferItem> files) : ITeensyCommand<SaveFilesResult>
    {
        public List<FileTransferItem> Files { get; } = files;
        public string? DeviceId { get; set; }
        public ISerialStateContext Serial { get; set; } = null!;
    }
}