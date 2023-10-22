using System.Collections.ObjectModel;

namespace TeensyRom.Core.Serial
{
    public static class TeensyConstants
    {
        public static readonly ReadOnlyCollection<byte> Ping_Bytes = Array.AsReadOnly(new byte[] { 0x64, 0x55 });
        public static readonly ReadOnlyCollection<byte> Reset_Bytes = Array.AsReadOnly(new byte[] { 0x64, 0xEE });


        public const UInt16 List_Directory_Token = 0x64DD;
        public const UInt16 Start_Directory_List_Token = 0x5A5A;
        public const UInt16 End_Directory_List_Token = 0xA5A5;

        public const UInt16 Send_File_Token = 0x64BB;
        public const UInt16 Legacy_Send_File_Token = 0x64AA;
        public const UInt16 Ack_Token = 0x64CC;
        public const UInt16 Fail_Token = 0x9B7F;
        public const UInt32 Sd_Card_Token = 1U;
        public const UInt32 Usb_Stick_Token = 0U;
    }
}
