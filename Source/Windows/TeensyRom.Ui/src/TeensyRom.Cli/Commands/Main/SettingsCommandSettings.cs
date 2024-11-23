using Spectre.Console.Cli;

namespace TeensyRom.Cli.Commands.Main
{
    internal class SettingsCommandSettings : CommandSettings 
    {
        public static string Description => "Change your global settings.";
        public static string Example => "settings";
    }
}