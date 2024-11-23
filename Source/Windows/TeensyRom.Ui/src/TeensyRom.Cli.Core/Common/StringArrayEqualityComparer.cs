using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Cli.Core.Common
{
    /// <summary>
    /// Compares two string arrays for equality.
    /// </summary>
    public class StringArrayEqualityComparer : IEqualityComparer<string[]>
    {
        public bool Equals(string[]? x, string[]? y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x == null || y == null) return false;

            return x.SequenceEqual(y);
        }

        public int GetHashCode(string[] obj)
        {
            if (obj == null) return 0;

            unchecked
            {
                int hash = 17;

                foreach (var item in obj)
                {
                    hash = hash * 23 + (item?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
    }
}
