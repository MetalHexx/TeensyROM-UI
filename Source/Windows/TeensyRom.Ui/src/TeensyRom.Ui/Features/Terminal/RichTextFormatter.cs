using System.Windows.Documents;
using System.Windows.Media;
using System.Text.RegularExpressions;
using TeensyRom.Core.Logging;

namespace TeensyRom.Ui.Features.Terminal
{
    public static class RichTextFormatter
    {
        private static Color TryGetColorFromString(string colorName)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(colorName);
            }
            catch
            {
                return Colors.White;
            }
        }
        public static Paragraph ToRichText(this string logMessage)
        {
            Paragraph paragraph = new();
            var matches = logMessage.GetColorMatches();

            int lastMatchEnd = 0;
            foreach (Match match in matches)
            {
                if (match.Groups.Count != 3) continue;

                if (match.Index > lastMatchEnd)
                {
                    paragraph.Inlines.Add(new Run(logMessage.Substring(lastMatchEnd, match.Index - lastMatchEnd)));
                }

                string colorName = match.Groups[1].Value;
                string message = match.Groups[2].Value;

                var color = TryGetColorFromString(colorName);
                var brush = new SolidColorBrush(color);

                var run = new Run(message) { Foreground = brush };
                paragraph.Inlines.Add(run);

                lastMatchEnd = match.Index + match.Length;
            }

            if (lastMatchEnd < logMessage.Length)
            {
                paragraph.Inlines.Add(new Run(logMessage[lastMatchEnd..]));
            }

            return paragraph;
        }
    }
}
