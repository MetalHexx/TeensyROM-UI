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
    public class MuteSidVoicesCommand(VoiceState muteVoice1, VoiceState muteVoice2, VoiceState muteVoice3) : IRequest<MuteSidVoicesResult>
    {
        public VoiceState Voice1Enabled { get; } = muteVoice1;
        public VoiceState Voice2Enabled { get; } = muteVoice2;
        public VoiceState Voice3Enabled { get; } = muteVoice3;
    }
}
