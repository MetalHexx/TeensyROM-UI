using System.Windows.Documents;
using System.Windows.Media;
using System.Text.RegularExpressions;
using TeensyRom.Ui.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using TeensyRom.Core.Logging;

namespace TeensyRom.Ui.Features.Terminal
{
    public static class RichTextFormatter
    {
        // Cache colors to avoid repeated parsing
        private static readonly Dictionary<string, SolidColorBrush> _cachedBrushes = new();
        
        // Default brushes for common colors
        private static readonly SolidColorBrush _defaultBrush = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush _yellowBrush = new SolidColorBrush(Colors.Yellow);
        
        // Color formatting using the LogHelper's regex-based color extraction
        public static Paragraph ToRichText(this string logMessage)
        {
            var paragraph = new Paragraph();
            
            try
            {
                // Special handling for system messages only
                if (logMessage.StartsWith("[Skipped") || logMessage.Contains("logs waiting in queue"))
                {
                    var run = new Run(logMessage)
                    {
                        Foreground = _yellowBrush,
                        FontWeight = FontWeights.Bold
                    };
                    paragraph.Inlines.Add(run);
                    return paragraph;
                }
                
                // Use the LogHelper's regex to match color tags
                var matches = logMessage.GetColorMatches();
                
                // If no matches found, just add the text as plain text
                if (matches.Count == 0)
                {
                    paragraph.Inlines.Add(new Run(logMessage));
                    return paragraph;
                }
                
                // Process each match and the text between matches
                int lastEnd = 0;
                foreach (Match match in matches)
                {
                    // Add any text before this color tag
                    if (match.Index > lastEnd)
                    {
                        string textBefore = logMessage.Substring(lastEnd, match.Index - lastEnd);
                        paragraph.Inlines.Add(new Run(textBefore));
                    }
                    
                    // Process the color tag
                    if (match.Groups.Count >= 3)
                    {
                        string colorCode = match.Groups[1].Value;
                        string coloredText = match.Groups[2].Value;
                        
                        // Get or create the brush
                        if (!_cachedBrushes.TryGetValue(colorCode, out var brush))
                        {
                            try
                            {
                                var color = (Color)ColorConverter.ConvertFromString(colorCode);
                                brush = new SolidColorBrush(color);
                                _cachedBrushes[colorCode] = brush;
                            }
                            catch
                            {
                                brush = _defaultBrush;
                            }
                        }
                        
                        // Add the colored text
                        var run = new Run(coloredText) { Foreground = brush };
                        paragraph.Inlines.Add(run);
                    }
                    
                    // Update lastEnd
                    lastEnd = match.Index + match.Length;
                }
                
                // Add any remaining text after the last color tag
                if (lastEnd < logMessage.Length)
                {
                    string textAfter = logMessage.Substring(lastEnd);
                    paragraph.Inlines.Add(new Run(textAfter));
                }
            }
            catch (Exception)
            {
                // Fall back to plain text on error
                paragraph.Inlines.Clear();
                paragraph.Inlines.Add(new Run(logMessage));
            }
            
            return paragraph;
        }
    }
}
