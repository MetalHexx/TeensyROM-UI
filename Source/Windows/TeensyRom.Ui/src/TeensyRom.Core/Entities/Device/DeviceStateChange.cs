using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Entities.Device
{
    public record DeviceStateChange(string DeviceId, ISerialState State);
}
