using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Music.Midi
{
    public static class MidiEventExtensions
    {
        public static double GetRelativeCCDelta(this MidiEvent e, double amtToAdd) => (e.Value - 64) * amtToAdd;
    }
}
