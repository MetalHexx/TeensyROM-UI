using System.Collections.ObjectModel;

namespace TeensyRom.Core.Serial.Constants
{
    public static class TeensyConstants
    {
        public static readonly ReadOnlyCollection<byte> Ping_Bytes = Array.AsReadOnly(new byte[] { 0x64, 0x55 });
        public static readonly ReadOnlyCollection<byte> Reset_Bytes = Array.AsReadOnly(new byte[] { 0x64, 0xEE });


        public const ushort List_Directory_Token = 0x64DD;
        public const ushort Start_Directory_List_Token = 0x5A5A;
        public const ushort End_Directory_List_Token = 0xA5A5;

        public const ushort Send_File_Token = 0x64BB;
        public const ushort Legacy_Send_File_Token = 0x64AA;
        public const ushort Ack_Token = 0x64CC;
        public const ushort Fail_Token = 0x9B7F;
        public const uint Sd_Card_Token = 1U;
        public const uint Usb_Stick_Token = 0U;
    }
}
