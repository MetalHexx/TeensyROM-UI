using System.Collections.ObjectModel;

namespace TeensyRom.Cli.Core.Serial
{
    public static class TeensyByteToken
    {
        public static readonly ReadOnlyCollection<byte> Ping_Bytes = Array.AsReadOnly(new byte[] { 0x64, 0x55 });
        public static readonly ReadOnlyCollection<byte> Reset_Bytes = Array.AsReadOnly(new byte[] { 0x64, 0xEE });
    }
}
