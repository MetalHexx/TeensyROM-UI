using System.Text;
using System.Text.RegularExpressions;
using Spectre.Console;
using TeensyRom.Core.Common;

namespace TeensyRom.Cli.Helpers
{
    /// <summary>
    /// Helper class that assists with adding some color to cli output
    /// </summary>
    public static class RadHelper
    {
        public static MarkupTheme Theme => new MarkupTheme(
            Primary: new MarkupColor(Color.Fuchsia, "fuchsia"),
            Secondary: new MarkupColor(Color.Aqua, "aqua"),
            Highlight: new MarkupColor(Color.White, "white"),
            Dark: new MarkupColor(Color.Grey, "grey"),
            Error: new MarkupColor(Color.Red, "red"));

        /// <summary>
        /// Renders the main cli logo
        /// </summary>
        /// <param name="fontPath">Font to use for the logo</param>
        public static void RenderLogo(string text, string fontPath)
        {
            var font = FigletFont.Load(fontPath.GetOsFriendlyPath());

            AnsiConsole.Write(new FigletText(font, text)
                .Color(Theme.Primary.Color));
        }
        /// <summary>
        /// Allows you to AddColumn with a table and pass a delegate
        /// </summary>
        public static Table AddColumn(this Table table, TableColumn column, Action<TableColumn>? configure = null)
        {
            configure?.Invoke(column);
            table.AddColumn(column);
            return table;
        }

        /// <summary>
        /// Formats message with primary color
        /// </summary>
        public static string AddPrimaryColor(this string message)
        {
            return $"[{Theme.Primary}]{message.EscapeBrackets()}[/]";
        }



        /// <summary>
        /// Formats message with secondary color
        /// </summary>
        public static string AddSecondaryColor(this string message)
        {
            return $"[{Theme.Secondary}]{message.EscapeBrackets()}[/]";
        }

        /// <summary>
        /// Formats message strings for cli output
        /// </summary>
        public static string AddHighlights(this string message)
        {
            var stringBuilder = new StringBuilder();
            var theme = Theme;

            foreach (var character in message)
            {
                var charString = character.ToString();
                string markupString = charString switch
                {
                    "." => $"[{theme.Secondary}]{charString}[/]",
                    "*" => $"[{theme.Secondary}]{charString}[/]",
                    "-" => $"[{theme.Secondary}]{charString}[/]",
                    "+" => $"[{theme.Secondary}]{charString}[/]",
                    "/" => $"[{theme.Secondary}]{charString}[/]",
                    "\\" => $"[{theme.Secondary}]{charString}[/]",
                    "_" => $"[{theme.Secondary}]{charString}[/]",
                    "[" => $"{charString}",
                    "]" => $"{charString}",
                    _ when Regex.IsMatch(charString, @"\W") => $"[{theme.Highlight}]{charString}[/]",
                    _ when Regex.IsMatch(charString, @"\d") => $"[{theme.Secondary}]{charString}[/]",
                    _ => $"[{theme.Primary}]{charString}[/]",
                };
                stringBuilder.Append(markupString);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Formats path strings for cli output
        /// </summary>
        public static string ToL33tPath(this string message)
        {
            var theme = Theme;
            var stringBuilder = new StringBuilder();

            foreach (var character in message)
            {
                var charString = character.ToString();
                string markupString = charString switch
                {
                    "[" => $"{charString}",
                    "]" => $"{charString}",
                    _ when Regex.IsMatch(charString, @"\\") => $"[{theme.Secondary}]{charString}[/]",
                    _ => $"[{theme.Highlight}]{charString}[/]",
                };

                stringBuilder.Append(markupString);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Writes a message with horizontal rule
        /// </summary>
        /// <param name="message"></param>
        public static void WriteHorizonalRule(string message, Justify justify)
        {
            var rule = new Rule($"-={message.EscapeBrackets()}=-".AddHighlights());
            rule.Justification = justify;
            AnsiConsole.Write(rule);
            AnsiConsole.WriteLine();
        }

        /// <summary>
        /// Writes a message
        /// </summary>
        /// <param name="message"></param>
        public static void WriteTitle(string message)
        {
            AnsiConsole.MarkupLine($"-={message.EscapeBrackets()}=-".AddHighlights());
        }

        /// <summary>
        /// Writes a message
        /// </summary>
        /// <param name="message"></param>


        /// <summary>
        /// Writes a message
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLine(string message = "")
        {
            AnsiConsole.MarkupLine(message.AddHighlights());
        }

        /// <summary>
        /// Displays an error message
        /// </summary>
        /// <param name="message"></param>
        public static void WriteError(string message)
        {
            AnsiConsole.MarkupLine($"[#cc666c]{message.EscapeBrackets()}[/]");
        }

        /// <summary>
        /// Writes a message with bullet
        /// </summary>
        /// <param name="message"></param>
        public static void WriteBullet(string message, int indent = 1)
        {
            var indentString = " ";

            for (int i = 0; i < indent; i++)
            {
                indentString += " ";
            }

            message = $"{indentString}- {message}";
            AnsiConsole.MarkupLine(message.AddHighlights().EscapeBrackets());
        }

        public static Progress AddTheme(this Progress progress)
        {
            var theme = Theme;
            return progress.Columns(
            [
                new TaskDescriptionColumn(),
                new ProgressBarColumn()
                    .FinishedStyle(new Style(foreground: theme.Primary.Color))
                    .RemainingStyle(new Style(foreground: theme.Secondary.Color)),
                new PercentageColumn()
                    .CompletedStyle(new Style(foreground: theme.Highlight.Color)),
                new SpinnerColumn(Spinner.Known.Balloon2)
                    .Style(new Style(foreground: theme.Secondary.Color)),
            ]);
        }

        public static void WriteMenu(string title, string description, params string[] bullets) 
        {
            var table = new Table()
               .BorderColor(Theme.Secondary.Color)
               .Border(TableBorder.Rounded)
               .Expand()
               .AddColumn(title.AddHighlights())
               .AddRow(description);

            if (bullets.Any()) 
            {
                table.AddEmptyRow();

                foreach (var bullet in bullets)
                {
                    table.AddRow($"* {AddHighlights(bullet)}");
                }                
            }
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        public static Table WriteHelpTable((string Header1, string Header2) headers, List<(string Key, string Value)> rows) 
        {
            var table = new Table()
                .BorderColor(Theme.Secondary.Color)
                .Border(TableBorder.Rounded)
                .Expand()
                .AddColumn(headers.Header1)
                .AddColumn(headers.Header2);

            foreach (var row in rows) 
            {
                table.AddRow(row.Key.EscapeBrackets().AddHighlights(), row.Value.EscapeBrackets().AddHighlights());
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            return table;
        }

        public static Table WriteDynamicTable(IEnumerable<string> headers, IEnumerable<IEnumerable<string>> rows)
        {

            var table = new Table()                
                .BorderColor(Theme.Secondary.Color)
                .Border(TableBorder.Rounded)
                .ShowRowSeparators()
                .Expand();

            foreach (var header in headers) 
            {
                table.AddColumn(header);
            }
            foreach (var row in rows)
            {
                var highlightedRows = row
                    .Select(v => v.EscapeBrackets().AddHighlights())
                    .ToArray();

                table.AddRow(highlightedRows);
            }
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            return table;
        }

        public static string EscapeBrackets(this string message) => message.Replace("[", "[[").Replace("]", "]]");
        public static string UnescapeBrackets(this string message) => message.Replace("[[", "[").Replace("]]", "]");
        public const string ClearHack = "                                                                          ";
    }
}
