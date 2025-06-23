using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Common
{
    public static class ByteExtensions
    {
        public static ushort ToInt16(this byte[] bytes) => (ushort)(bytes[1] * 256 + bytes[0]);

        /// Converts a byte array to a UTF-8 string, optionally trimming the last few bytes.        
        public static string ToUtf8(this byte[] bytes, int trimEndBytes = 0)
        {
            if (bytes == null || bytes.Length == 0) return string.Empty;

            int length = bytes.Length;

            if (trimEndBytes > 0 && trimEndBytes < length)
            {
                length -= trimEndBytes;
            }

            return Encoding.UTF8.GetString(bytes, 0, length);
        }

        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}
