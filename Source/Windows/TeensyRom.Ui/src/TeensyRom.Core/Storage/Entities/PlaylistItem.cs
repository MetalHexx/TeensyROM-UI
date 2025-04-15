using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Music;

namespace TeensyRom.Core.Storage.Entities
{
    public class PlaylistItem
    {
        public string FilePath { get; set; } = string.Empty;
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
