using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TeensyRom.Core.Common
{
    public static class ByteExtensions
    {
        public static string ToLogString(this byte[] bytes) => new(bytes.Select(b => (char)b).ToArray());
        public static ushort ToInt16(this byte[] bytes) => (ushort)(bytes[1] * 256 + bytes[0]);

        /// <summary>
        /// Converts a byte array to a UTF-8 string, optionally trimming the last few bytes
        /// </summary>
        /// <param name="bytes">bytes to encode into a string</param>
        /// <param name="trimEndBytes">Kind of hacky, trims characters off the string if expecting things like carriage returns, etc.</param>        
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

        public static ushort CalculateChecksum(this byte[] data)
        {
            uint checksum = 0;
            foreach (var b in data)
            {
                checksum += b;
            }
            return (ushort)(checksum & 0xffff);
        }

        public static T? Deserialize<T>(this byte[]? jsonBytes)
        {
            if (jsonBytes == null || jsonBytes.Length == 0)
                return default;

            try
            {
                var json = Encoding.UTF8.GetString(jsonBytes);

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });
            }
            catch
            {
                return default;
            }
        }

        public static byte[]? Serialize<T>(this T? value)
        {
            if (value == null) return null;

            try
            {
                var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                return Encoding.UTF8.GetBytes(json);
            }
            catch
            {
                return [];
            }
        }

    }
}
