using Spectre.Console;
using Spectre.Console.Cli;
using System.Reactive.Linq;
using System.Transactions;
using TeensyRom.Cli.Helpers;
using TeensyRom.Cli.Services;
using TeensyRom.Cli.Core.Serial.State;
using TeensyRom.Cli.Core.Settings;

namespace TeensyRom.Cli.Commands.Main
{
    internal class SettingsCommand(ISerialStateContext serial, IPlayerService player, ISettingsService settingsService, ICliLoggingService logService) : AsyncCommand<SettingsCommandSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, SettingsCommandSettings _)
        {
            player.StopStream();

            var choice = string.Empty;

            do
            {
                RadHelper.WriteMenu("Settings", "Change your global settings.", []);

                var settings = settingsService.GetSettings();

                RadHelper.WriteDynamicTable(["Setting", "Value", "Description"],
                [
                    ["Storage Device", settings.StorageType.ToString(), "Default value to use for your selected storage device."],
                    ["Always Prompt Storage", settings.AlwaysPromptStorage.ToString(), "Determines if you're always prompted to select the storage device."],
                    ["Debug Logs Enabled", logService.Enabled.ToString(), "Enables verbose logs for debugging." ]
                ]);

                choice = PromptHelper.ChoicePrompt("Choose wisely", new List<string> { "Storage Device", "Always Prompt Storage", "Toggle Debug Logs", "Leave Settings" });

                switch (choice)
                {
                    case "Storage Device":
                        settings.StorageType = CommandHelper.PromptForStorageType(settings.StorageType.ToString(), true);
                        break;

                    case "Always Prompt Storage":
                        settings.AlwaysPromptStorage = PromptHelper.Confirm("Always Prompt Storage", settings.AlwaysPromptStorage);
                        break;

                    case "Toggle Debug Logs":
                        settings.EnableDebugLogs = !settings.EnableDebugLogs;
                        logService.Enabled = settings.EnableDebugLogs;
                        break;
                }
                if (choice != "Leave Settings")
                {
                    settingsService.SaveSettings(settings);
                }
            } while (choice != "Leave Settings");

            RadHelper.WriteLine();
            return 0;
        }
    }
}