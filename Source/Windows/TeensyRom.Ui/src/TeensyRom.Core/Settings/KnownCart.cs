using TeensyRom.Core.Entities.Midi;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Settings
{
    public record KnownCart(string DeviceHash, string PnpDeviceId, string ComPort, string Name, MidiSettings MidiSettings, ILaunchableItem? LastFile);
}
