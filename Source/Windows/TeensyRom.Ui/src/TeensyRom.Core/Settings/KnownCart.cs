using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Music.Midi;

namespace TeensyRom.Core.Settings
{
    public record KnownCart(string DeviceHash, string PnpDeviceId, string ComPort, string Name, MidiSettings MidiSettings);
}
