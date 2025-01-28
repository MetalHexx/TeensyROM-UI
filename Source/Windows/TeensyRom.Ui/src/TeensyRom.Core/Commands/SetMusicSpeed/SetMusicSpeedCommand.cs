using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Music;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.SetMusicSpeed
{
    public class SetMusicSpeedCommand(double speed, MusicSpeedCurveTypes type) : IRequest<SetMusicSpeedResult>
    {
        public double Speed { get; } = speed;
        public MusicSpeedCurveTypes Type { get; } = type;
    }
}
