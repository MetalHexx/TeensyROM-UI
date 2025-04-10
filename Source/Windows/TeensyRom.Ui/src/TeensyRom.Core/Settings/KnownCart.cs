using TeensyRom.Core.Music.Midi;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Settings
{
    public record KnownCart(string DeviceHash, string PnpDeviceId, string ComPort, string Name, MidiSettings MidiSettings, ILaunchableItem? LastFile);
}
