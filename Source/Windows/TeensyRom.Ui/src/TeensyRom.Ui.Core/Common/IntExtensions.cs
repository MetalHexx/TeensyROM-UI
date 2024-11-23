using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Ui.Core.Common
{
    public static class IntExtensions
    {
        /// <summary>
        /// Tries to convert a uint to a valid int.
        /// </summary>
        /// <param name="value">The uint value to convert.</param>
        /// <param name="result">The resulting int value if the conversion is successful.</param>
        /// <returns>True if the conversion is successful; otherwise, false.</returns>
        public static bool TryParseInt(this uint value, out int result)
        {
            if (value <= int.MaxValue)
            {
                result = (int)value;
                return true;
            }
            else
            {
                result = 0;
                return false;
            }
        }
    }
}
