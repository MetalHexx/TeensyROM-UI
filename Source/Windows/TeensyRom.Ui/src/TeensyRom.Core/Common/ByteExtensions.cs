using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Common
{
    public static class ByteExtensions
    {
        public static string ToLogString(this byte[] bytes) => new(bytes.Select(b => (char)b).ToArray());
        public static ushort ToInt16(this byte[] bytes) => (ushort)(bytes[1] * 256 + bytes[0]);
        public static string ToAscii(this byte[] bytes) 
        {
            var dataString = Encoding.ASCII.GetString(bytes);

            if (string.IsNullOrWhiteSpace(dataString)) return string.Empty;

            return dataString;
        }

        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}
