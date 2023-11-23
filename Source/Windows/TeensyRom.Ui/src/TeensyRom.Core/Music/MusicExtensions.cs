using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Music
{
    public static class MusicExtensions
    {
        public static string EnsureNotEmpty(this string data, string emptyValue)
        {
            data = data.Replace("<?>", "");

            if (string.IsNullOrWhiteSpace(data)) data = emptyValue;

            return data;
        }
    }
}
