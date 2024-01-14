using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TeensyRom.Core.Logging
{
    public static partial class LogHelper
    {
        public static Regex LogRegEx => LogColorRegex();

        public static string WithColor(this string message, string hexColor)
        {
            return $"[Color:{hexColor}]{message}[/Color]\r\n";
        }
        public static string WithColor(this string message, Color color)
        {
            var hexColor = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            return $"[Color:{hexColor}]{message}[/Color]\r\n";
        }

        public static MatchCollection GetColorMatches(this string message) => LogRegEx.Matches(message);

        [GeneratedRegex(@"\[Color:(.*?)\](.*?)\[/Color\]")]
        private static partial Regex LogColorRegex();
    }
}
