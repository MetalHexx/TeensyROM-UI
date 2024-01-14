using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Common
{
    public static class StringExtensions
    {
        public static List<string> SplitAtCarriageReturn(this string message)
        {
            string[] delimiters = ["\r\n", "\n", "\r"];
            string[] parts = message.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            return new List<string>(parts);
        }

        public static string DropLastComma(this string message) => message.TrimEnd(',', ' ');
        public static string DropLastNewLine(this string message) => message.TrimEnd('\r', '\n', ' ');        
    }
}
