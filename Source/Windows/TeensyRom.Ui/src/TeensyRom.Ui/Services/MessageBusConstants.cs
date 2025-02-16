using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Ui.Services
{
    public static class MessageBusConstants
    {
        /// <summary>
        /// Message used to signal when the 1, 2, 3 keys are typed in for muting sid voices.
        /// </summary>
        public const string GlobalVoiceKeyMessageName = "GlobalVoiceKey";
    }
}
