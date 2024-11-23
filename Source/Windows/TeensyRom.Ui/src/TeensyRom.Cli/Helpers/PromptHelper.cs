using Spectre.Console;

namespace TeensyRom.Cli.Helpers
{
    /// <summary>
    /// A helper class to create consistent prompts and reduce noise
    /// </summary>
    internal static class PromptHelper
    {
        /// <summary>
        /// Renders a freeform text prompt entry with a length requirement
        /// </summary>
        /// <param name="prompt">The message prompt to show the user</param>
        /// <param name="length">The minimum length allowed from the users input</param>
        /// <param name="theme">Allows an override of the default theme for special cases</param>
        /// <returns>The user input</returns>
        public static string RequiredLengthTextPrompt(string prompt, int length)
        {
            var theme = RadHelper.Theme;

            return AnsiConsole.Prompt(
                new TextPrompt<string>($"[{theme.Primary}]{prompt}[/]")
                    .PromptStyle(theme.Primary.ToString())
                    .DefaultValueStyle(theme.Primary.ToString())
                    .Validate(input =>
                    {
                        if (input.Length < length) return ValidationResult.Error($"[{theme.Error}]Length must be greater than {length}[/]");
                        return ValidationResult.Success();
                    }));
        }

        /// <summary>
        /// Renders a freeform text prompt entry with a length requirement
        /// </summary>
        /// <param name="prompt">The message prompt to show the user</param>
        /// <param name="length">The minimum length allowed from the users input</param>
        /// <param name="theme">Allows an override of the default theme for special cases</param>
        /// <returns>The user input</returns>
        public static string DefaultValueTextPrompt(string prompt, int length, string defaultValue = "")
        {
            var theme = RadHelper.Theme;

            return AnsiConsole.Prompt(
                new TextPrompt<string>($"[{theme.Primary}]{prompt}[/]")
                    .PromptStyle(theme.Secondary.ToString())
                    .DefaultValue(defaultValue)
                    .DefaultValueStyle(theme.Primary.ToString())
                    .Validate(input =>
                    {
                        if (input.Length < length && string.IsNullOrWhiteSpace(defaultValue))
                        {
                            return ValidationResult.Error($"[{theme.Error}]Length must be greater than {length}[/]");
                        }
                        return ValidationResult.Success();
                    }));
        }

        /// <summary>
        /// Renders a true/false (yes/no) prompt with boolean result
        /// </summary>
        /// <param name="message">The message prompt to show the user</param>
        /// <param name="theme">Allows an override of the default theme for special cases</param>
        /// <returns>A boolean result based on user input</returns>
        public static bool Confirm(string message, bool defaultValue)
        {
            var theme = RadHelper.Theme;

            var yesNo = defaultValue
                ? $"[{theme.Secondary}](Y/n)[/]"
                : $"[{theme.Secondary}](y/N)[/]";

            var values = defaultValue
                ? new[] { "Yes", "No" }
                : new[] { "No", "Yes" };

            var input = AnsiConsole.Prompt
            (
                new SelectionPrompt<string>()
                    .Title($"[{theme.Primary}]{message}[/] {yesNo}")
                    .HighlightStyle(theme.Secondary.ToString())
                    .AddChoices(values)
            );

            AnsiConsole.MarkupLine($"[{theme.Primary}]{message}[/][{theme.Secondary}]{input}[/]");

            return input.Equals("Yes") ? true : false;
        }

        /// <summary>
        /// Renders a choice prompt with string choices and response
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="choices">The choices to show the user</param>
        /// <param name="theme">Allows an override of the default theme for special cases</param>
        /// <returns>Selected choice</returns>
        public static string ChoicePrompt(string message, List<string> choices)
        {
            var theme = RadHelper.Theme;

            var selection = AnsiConsole.Prompt
            (
                new SelectionPrompt<string>()
                    .Title($"[{theme.Primary}]{message}: [/]")                    
                    .HighlightStyle(theme.Secondary.ToString())
                    .AddChoices(choices)
            );

            AnsiConsole.MarkupLine($"[{theme.Primary}]{message}: [/][{theme.Secondary}]{selection}[/]");

            return selection;
        }

        /// <summary>
        /// Renders a choice prompt with string choices and response
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="choices">The choices to show the user</param>
        /// <param name="theme">Allows an override of the default theme for special cases</param>
        /// <returns>Selected choice</returns>
        public static string FilePrompt(string message, List<string> choices)
        {
            var theme = RadHelper.Theme;

            var sanitizedChoices = choices.Select(f => f.EscapeBrackets()).ToList();

            var selection = AnsiConsole.Prompt
            (
                new SelectionPrompt<string>()
                    .Title($"[{theme.Primary}]{message}: [/]")
                    .HighlightStyle(theme.Secondary.ToString())
                    .AddChoices(sanitizedChoices)
            );

            AnsiConsole.MarkupLine($"[{theme.Primary}]{message}: [/][{theme.Secondary}]{selection}[/]");

            return selection.UnescapeBrackets();
        }
    }
}
