using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Music.Midi
{
    public static class MidiEventExtensions
    {
        public static double GetRelativeValue_TwosComplement(this MidiEvent e, double amtToAdd) => (e.Value - 64) * amtToAdd;

        public static double GetRelativeValue_BinaryOffset(this MidiEvent e, double multiplier = 1.0)
        {
            if (e.Value == 0) return 0;

            return e.Value <= 64
                ? e.Value * multiplier 
                : (e.Value - 128) * multiplier;
        }
    }
}
