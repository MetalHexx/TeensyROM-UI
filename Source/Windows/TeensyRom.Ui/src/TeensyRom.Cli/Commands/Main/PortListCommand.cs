using Spectre.Console;
using Spectre.Console.Cli;
using System.Reactive.Linq;
using TeensyRom.Cli.Helpers;
using TeensyRom.Cli.Services;
using TeensyRom.Cli.Core.Serial.State;

namespace TeensyRom.Cli.Commands.Main
{
    internal class PortListSettings : CommandSettings
    {
        public static string Example => "ports";
        public static string Description => "List all available serial ports on the machine for troubleshooting.";
    }

    internal class PortListCommand(ISerialStateContext serial, IPlayerService player) : AsyncCommand<PortListSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, PortListSettings settings)
        {
            player.StopStream();

            RadHelper.WriteMenu("Port List", "Troubleshooting tool for listing all the available serial ports on the machine.", []);

            var ports = await serial.Ports.FirstAsync();

            RadHelper.WriteLine("Ports Found: ");
            AnsiConsole.WriteLine();

            foreach (var port in ports)
            {
                RadHelper.WriteLine(port);
            }
            RadHelper.WriteLine();
            return 0;
        }
    }
}