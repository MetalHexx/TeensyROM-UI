using TeensyRom.Core.Entities.Midi;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Serial
{
    public record Cart(string DeviceId, string ComPort, string Name, string? FwVersion, bool IsCompatible);
}
