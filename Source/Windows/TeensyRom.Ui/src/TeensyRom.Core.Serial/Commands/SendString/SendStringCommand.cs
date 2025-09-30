using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Serial.Commands;

namespace TeensyRom.Core.Commands.SendString
{
    public class SendStringCommand(string stringToSend, string? deviceId = null) : ITeensyCommand<SendStringResult>
    {
        public string StringToSend { get; } = stringToSend;
        public string? DeviceId { get; set; } = deviceId;
        public ISerialStateContext Serial { get; set; } = null!;
    }
}