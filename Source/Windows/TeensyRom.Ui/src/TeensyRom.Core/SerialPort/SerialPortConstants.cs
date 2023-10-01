using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Serial
{
    /// <summary>
    /// Serial port configuration constants
    /// </summary>
    public static class SerialPortConstants
    {
        public const int Read_Polling_Milliseconds = 100;
        public const int Read_Retry_Seconds = 5;
        public static readonly ReadOnlyCollection<byte> Ping_Bytes = Array.AsReadOnly(new byte[] { 0x64, 0x55 });
    }
}
