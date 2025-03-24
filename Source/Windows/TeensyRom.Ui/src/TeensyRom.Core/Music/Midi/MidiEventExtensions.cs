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

        public static double GetAbsoluteValueDelta(this MidiEvent e, double min, double max, double currentValue)
        {
            double normalized = e.Value / 127.0;

            double targetValue = min + (normalized * (max - min));

            return targetValue - currentValue;
        }
    }
}
