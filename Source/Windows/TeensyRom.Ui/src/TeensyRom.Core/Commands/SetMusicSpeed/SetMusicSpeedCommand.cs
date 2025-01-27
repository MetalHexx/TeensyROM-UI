using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.SetMusicSpeed
{
    public enum MusicSpeedType 
    {
        Linear,
        Logarithmic
    }
    public class SetMusicSpeedCommand(double speed, MusicSpeedType type) : IRequest<SetMusicSpeedResult>
    {
        public double Speed { get; } = speed;
        public MusicSpeedType Type { get; } = type;
    }
}
