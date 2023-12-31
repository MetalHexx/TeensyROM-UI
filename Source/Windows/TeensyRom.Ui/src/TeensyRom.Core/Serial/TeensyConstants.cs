using Ardalis.SmartEnum;
using System.Collections.ObjectModel;

namespace TeensyRom.Core.Serial
{
    public static class TeensyConstants
    {
        public static readonly ReadOnlyCollection<byte> Ping_Bytes = Array.AsReadOnly(new byte[] { 0x64, 0x55 });
        public static readonly ReadOnlyCollection<byte> Reset_Bytes = Array.AsReadOnly(new byte[] { 0x64, 0xEE });
        public const uint Sd_Card_Token = 1U;
        public const uint Usb_Stick_Token = 0U;
    }
}
