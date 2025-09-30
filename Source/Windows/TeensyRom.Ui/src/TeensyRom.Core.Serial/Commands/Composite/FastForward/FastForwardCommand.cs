using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Commands.MuteSidVoices;

namespace TeensyRom.Core.Serial.Commands.Composite.FastForward
{
    public class FastForwardCommand(string? deviceId = null) : ITeensyCommand<FastForwardResult>
    {
        public bool ShouldTogglePlay { get; set; } = false;
        public bool ShouldMuteVoices { get; set; } = false;
        public double Speed { get; set; }
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
    }
}
