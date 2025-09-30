using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial.Commands;

namespace TeensyRom.Core.Commands.MuteSidVoices
{
    public class MuteSidVoicesCommand(VoiceState muteVoice1, VoiceState muteVoice2, VoiceState muteVoice3, string? deviceId = null) : ITeensyCommand<MuteSidVoicesResult>
    {
        public VoiceState Voice1Enabled { get; } = muteVoice1;
        public VoiceState Voice2Enabled { get; } = muteVoice2;
        public VoiceState Voice3Enabled { get; } = muteVoice3;
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
    }
}
