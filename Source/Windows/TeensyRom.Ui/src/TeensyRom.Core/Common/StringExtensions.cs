using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
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

        public static string StripCarriageReturnsAndExtraWhitespace(this string input)
        {
            string noCarriageReturnsOrNewLines = Regex.Replace(input, @"\r\n?|\n", " ");
            string cleanedString = Regex.Replace(noCarriageReturnsOrNewLines, @"\s+", " ");
            return cleanedString.Trim();
        }

        public static string RemoveFirstOccurrence(this string input, string pattern)
        {
            int index = input.IndexOf(pattern);
            return index < 0 ? input : input.Remove(index, pattern.Length);
        }

        public static string DropLastComma(this string message) => message.TrimEnd(',', ' ');
        public static string DropLastNewLine(this string message) => message.TrimEnd('\r', '\n', ' ');

        public static List<string> GetRemainingPathSegments(this string fullPath, string basePath)
        {
            var fullPathItems = fullPath.ToPathArray().ToList();
            var basePathItems = basePath.ToPathArray().ToList();

            fullPathItems = fullPathItems.Select(x => x.ToLower()).ToList();
            basePathItems = basePathItems.Select(x => x.ToLower()).ToList();

            int startIndex = FindStartIndexOfBasePath(fullPathItems, basePathItems);
            if (startIndex == -1)
            {
                return [];
            }
            int remainingPathLength = fullPathItems.Count - (startIndex + basePathItems.Count) - 1;

            if (remainingPathLength <= 0)
            {
                return [];
            }
            List<string> remainingPathSegments = fullPathItems.GetRange(startIndex + basePathItems.Count, remainingPathLength);

            return remainingPathSegments;
        }

        private static int FindStartIndexOfBasePath(List<string> fullPath, List<string> basePath)
        {
            for (int i = 0; i <= fullPath.Count - basePath.Count; i++)
            {
                var currentSlice = fullPath.GetRange(i, basePath.Count);

                if (Enumerable.SequenceEqual(currentSlice, basePath))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}