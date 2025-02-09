using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands.SetMusicSpeed;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music;

namespace TeensyRom.Core.Commands.MuteSidVoices
{
    public class MuteSidVoicesCommand(bool muteVoice1, bool muteVoice2, bool muteVoice3) : IRequest<MuteSidVoicesResult>
    {
        public bool Voice1Enabled { get; } = !muteVoice1;
        public bool Voice2Enabled { get; } = !muteVoice2;
        public bool Voice3Enabled { get; } = !muteVoice3;

        public byte VoiceMuteInfo => (byte)(
            (Voice1Enabled ? 0 : 1) << 0 |
            (Voice2Enabled ? 0 : 1) << 1 |
            (Voice3Enabled ? 0 : 1) << 2
        );
    }
}
