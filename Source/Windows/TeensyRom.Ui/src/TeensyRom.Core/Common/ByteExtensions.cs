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
    }
}
