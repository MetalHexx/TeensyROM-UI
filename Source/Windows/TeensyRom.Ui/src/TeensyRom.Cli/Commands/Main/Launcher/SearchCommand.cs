using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Reactive.Linq;
using TeensyRom.Cli.Helpers;
using TeensyRom.Cli.Services;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;

namespace TeensyRom.Cli.Commands.Main.Launcher
{
    internal class SearchSettings : LaunchSettings, IClearableSettings, IRequiresConnection
    {
        [Description("Storage device to search. (sd or usb)")]
        [CommandOption("-s|--storage")]
        public string StorageDevice { get; set; } = string.Empty;

        [Description("Search query.  Ex: \"iron maiden aces high\"")]
        [CommandOption("-q|--query")]
        public string Query { get; set; } = string.Empty;

        [Description("The type of files that will be searched. (all, music, games or images).")]
        [CommandOption("-f|--filter")]
        public string Filter { get; set; } = string.Empty;        

        [Description("Timer used for Games, Images and SIDs. (No, 3m, 5m, 15m, 30m, 1h, Turbo)")]
        [CommandOption("-t|--timer")]
        public string Timer { get; set; } = string.Empty;

        [Description("Timer should override SID song length")]
        [CommandOption("--override")]
        public bool? SidOverride { get; set; }

        public new static string Example => "launch search -s sd -q \"iron maiden aces high\"";
        public new static string Description => "Search for a file.";

        public new void ClearSettings()
        {
            StorageDevice = string.Empty;
            Filter = string.Empty;
            Timer = string.Empty;
            SidOverride = null;
            Query = string.Empty;
        }

        public override ValidationResult Validate()
        {
            if (!StorageDevice.Equals(string.Empty) && !StorageDevice.IsValueValid(["sd", "usb"]))
            {
                return ValidationResult.Error($"Storage device must be 'sd' or 'usb'.");
            }
            return base.Validate();
        }
    }

    internal class SearchCommand(ISerialStateContext serial, ICachedStorageService storage, ITypeResolver resolver, IPlayerService player, ISettingsService settingsService) : AsyncCommand<SearchSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, SearchSettings settings)
        {
            var playerCommand = resolver.Resolve(typeof(PlayerCommand)) as PlayerCommand;

            player.StopStream();

            RadHelper.WriteMenu("Search Stream", "Index your files to fatten your search. ;)");
            RadHelper.WriteHelpTable(("Example Search", "Description"),
            [
                ("iron maiden aces high", "Searches for ANY term individually."),
                ("iron maiden \"aces high\"", "Add quotes to match a phrase"),
                ("+iron maiden aces high", "\"iron\" MUST have a match in every search result"),
                ("+\"iron maiden\" \"aces high\"", "\"iron maiden\" MUST have a match in every search result"),
            ]);

            var globalSettings = settingsService.GetSettings();

            settings.StorageDevice = string.IsNullOrWhiteSpace(settings.StorageDevice)
                ? globalSettings.StorageType.ToString()
                : settings.StorageDevice;

            var storageType = CommandHelper.PromptForStorageType(settings.StorageDevice, promptAlways: globalSettings.AlwaysPromptStorage);

            if (globalSettings.AlwaysPromptStorage)
            {
                storage.SwitchStorage(storageType);
            }

            if (string.IsNullOrWhiteSpace(settings.Query))
            {
                settings.Query = PromptHelper.DefaultValueTextPrompt("Search Terms:", 2, "iron maiden");
                RadHelper.WriteLine();
            }

            var validation = settings.Validate();

            if (!validation.Successful)
            {
                RadHelper.WriteError(validation?.Message ?? "Validation error");
                return 0;
            }

            var filterType = CommandHelper.PromptForFilterType(settings.Filter);

            if (filterType is TeensyFilterType.All or TeensyFilterType.Games or TeensyFilterType.Images)
            {
                player.SetStreamTime(
                    CommandHelper.PromptGameTimer(settings.Timer));

                if (filterType is TeensyFilterType.All)
                {
                    var overrideSid = settings.SidOverride switch
                    {
                        null => "",
                        true => "Timer Override",
                        false => "Song Length"
                    };
                    var sidTimer = CommandHelper.PromptSidTimer(overrideSid);
                    player.SetSidTimer(sidTimer);
                }
            }
            var searchResults = storage.Search(settings.Query, StorageHelper.GetFileTypes(filterType));

            if (!searchResults.Any())
            {
                RadHelper.WriteError("No files found with the specified search criteria");
                AnsiConsole.WriteLine();
                return 0;
            }

            var fileName = PromptHelper.FilePrompt("Select File", searchResults
                .Select(f => f.Name)
                .ToList());

            AnsiConsole.WriteLine();

            var file = searchResults.First(f => f.Name == fileName);
            player.SetSearchMode(settings.Query);
            player.SetStorage(storageType);

            await player.LaunchFromDirectory(file.Path);

            if (playerCommand is not null)
            {
                playerCommand.Execute(context, new());
            }
            return 0;
        }
    }
}