using Spectre.Console.Cli;
using TeensyRom.Cli.Helpers;
using TeensyRom.Cli.Services;
using TeensyRom.Cli.Core.Serial.State;

namespace TeensyRom.Cli.Commands.Main
{
    internal class CreditsSettings : CommandSettings
    {
        public static string Example => "credits";
        public static string Description => "Credits to all the folks who helped with ideas and testing.";
    }
    internal class CreditsCommand() : Command<CreditsSettings>
    {
        public override int Execute(CommandContext context, CreditsSettings settings)
        {
            RadHelper.WriteMenu("Credits", "Special thanks to the following folks for their contributions to the Desktop UI and CLI projects:", 
            [
                "Wife -> Support -> Listening to my nonstop chatter about TeensyROM",
                "Son -> Support -> UI Feedback -> Feature Inspiration",
                "Travis Smith -> Inventor of TeensyROM -> Support -> Testing",
                "DivertigO -> Testing: Desktop UI and CLI",
                "Avrilcadabra -> Testing: Desktop UI and CLI",                
                "Richard -> Testing: Desktop UI",
                "jcook793 -> Testing: CLI (MacOS)",
                "mad -> Testing: CLI (MacOS)",                
            ]);
            return 0;
        }
    }
}