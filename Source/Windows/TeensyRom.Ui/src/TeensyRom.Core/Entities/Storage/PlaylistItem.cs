using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Music;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Entities.Storage
{
    public class PlaylistItem
    {
        public int Order { get; set; } = 0;
        public FilePath FilePath { get; set; } = new FilePath(string.Empty);
        public double DefaultSpeed { get; set; } = 0;
        public MusicSpeedCurveTypes DefaultSpeedCurve { get; set; } = MusicSpeedCurveTypes.Linear;

        public PlaylistItem Clone() => new()
        {
            FilePath = FilePath,
            DefaultSpeed = DefaultSpeed,
            DefaultSpeedCurve = DefaultSpeedCurve
        };
    }
}
