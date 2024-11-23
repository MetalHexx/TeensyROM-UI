using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Cli.Core.Music.Sid
{
    public static class SidExtensions
    {
        public static string EnsureNotEmpty(this string sidMetadata, string emptyValue)
        {
            sidMetadata = sidMetadata.Replace("<?>", "");

            if (string.IsNullOrWhiteSpace(sidMetadata)) sidMetadata = emptyValue;

            return sidMetadata;
        }
    }
}
