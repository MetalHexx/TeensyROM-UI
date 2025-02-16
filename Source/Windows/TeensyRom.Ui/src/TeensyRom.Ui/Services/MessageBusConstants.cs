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
        public const string SidVoiceMuteKeyPressed = "SidVoiceKeyPressed";
        public const string SidSpeedIncreaseKeyPressed = "SidSpeedIncreaseKeyPressed";
        public const string SidSpeedDecreaseKeyPressed = "SidSpeedDecreaseKeyPressed";
        public const string SidSpeedIncrease50KeyPressed = "SidSpeedIncrease50KeyPressed";
        public const string SidSpeedDecrease50KeyPressed = "SidSpeedDecrease50KeyPressed";
        public const string SidSpeedDefaultKeyPressed = "SidSpeedDefaultKeyPressed";
        public const string PlayerPlayPausedKeyPressed = "PlayerPlayPausedKeyPressed";
        public const string PlayerStopKeyPressed = "PlayerStopKeyPressed";
        public const string PlayerNextTrackKeyPressed = "PlayerNextTrackKeyPressed";
        public const string PlayerPreviousTrackKeyPressed = "PlayerPreviousTrackKeyPressed";
    }
}
