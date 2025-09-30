using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Serial.Commands;

namespace TeensyRom.Core.Serial.Commands.ToggleMusic
{
    public class ToggleMusicCommand(string? deviceId = null) : ITeensyCommand<ToggleMusicResult>
    {
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
    }
}
