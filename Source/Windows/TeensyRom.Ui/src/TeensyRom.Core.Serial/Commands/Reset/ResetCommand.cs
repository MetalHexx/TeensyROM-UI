using MediatR;
using System.IO;
using System.Reactive;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Serial.Commands;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Commands
{
    public class ResetCommand : ITeensyCommand<ResetResult>
    {
        public string? DeviceId { get; set; }
        public ISerialStateContext Serial { get; set; } = null!;
    }
}
