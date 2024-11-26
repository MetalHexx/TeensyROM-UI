using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using TeensyRom.Cli.Helpers;
using TeensyRom.Cli.Services;
using TeensyRom.Cli.Core.Storage.Services;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;

namespace TeensyRom.Cli.Commands.Main.Launcher
{
    internal class RandomSettings : LaunchSettings, IClearableSettings, IRequiresConnection
    {
        [Description("Storage device of file to launch. (sd or usb)")]
        [CommandOption("-s|--storage")]
        public string StorageDevice { get; set; } = string.Empty;

        [Description("The type of files that will be played. (all, music, games or images).")]
        [CommandOption("-f|--filter")]
        public string Filter { get; set; } = string.Empty;

        [Description("Files will only be played from this directory and subdirs.")]
        [CommandOption("-d|--directory")]
        public string PinnedDirectory { get; set; } = string.Empty;

        [Description("Timer used for Games, Images and SIDs. (No, 3m, 5m, 15m, 30m, 1h, Turbo)")]
        [CommandOption("-t|--timer")]
        public string Timer { get; set; } = string.Empty;

        [Description("Timer should override SID song length")]
        [CommandOption("--override")]        
        public bool? SidOverride { get; set; }

        public new static string Example = "launch random -s sd -f all -t 3m -d /demos --override";
        public new static string Description = "Randomly streams files from the specified directory and it's subdirectories.";

        public new void ClearSettings()
        {
            StorageDevice = string.Empty;
            Filter = string.Empty;
            PinnedDirectory = string.Empty;
            Timer = string.Empty;
            SidOverride = null;
        }

        public override ValidationResult Validate()
        {
            var timerValidation = Timer.ValidateTimer();
            if (!timerValidation.Successful) return timerValidation;

            var storageValidation = StorageDevice.ValidateStorageDevice();

            if (!storageValidation.Successful) return storageValidation;

            var directoryValidation = PinnedDirectory.ValidateUnixPath();

            if (!directoryValidation.Successful) return directoryValidation;

            var filterValidation = Filter.ValidateFilter();

            if (!filterValidation.Successful) return filterValidation;

            return base.Validate();
        }
    }
    internal class RandomCommand(ISerialStateContext serial, ICachedStorageService storage, ILoggingService logService, ISettingsService settingsService, IPlayerService player, ITypeResolver resolver) : AsyncCommand<RandomSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, RandomSettings settings)
        {
            var playerCommand = resolver.Resolve(typeof(PlayerCommand)) as PlayerCommand;

            player.StopStream();

            RadHelper.WriteMenu("Random Stream", "Randomly streams files from the specified directory and it's subdirectories.",
            [
               "You can limit randomization to a directory and its subdirs. Ex: /demos.",
               "Games, images and demos can be streamed with a timer.",
               "SIDs can stream continuously on play length or timer.",                              
               "Index your files to fatten your random streams. ;)"
            ]);
            var globalSettings = settingsService.GetSettings();

            settings.StorageDevice = string.IsNullOrWhiteSpace(settings.StorageDevice)
                ? globalSettings.StorageType.ToString()
                : settings.StorageDevice;

            var storageType = CommandHelper.PromptForStorageType(settings.StorageDevice, promptAlways: globalSettings.AlwaysPromptStorage);
            player.SetStorage(storageType);

            if (globalSettings.AlwaysPromptStorage)
            {
                storage.SwitchStorage(storageType);                
            }
            settings.PinnedDirectory = CommandHelper.PromptForDirectoryPath(settings.PinnedDirectory, "/");
            var filterType = CommandHelper.PromptForFilterType(settings.Filter);

            if (filterType is TeensyFilterType.All or TeensyFilterType.Games or TeensyFilterType.Images)
            {
                player.SetStreamTime(CommandHelper.PromptGameTimer(settings.Timer));

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

            if (!settings.ValidateSettings()) return -1;

            player.SetStorage(storageType);
            player.SetDirectoryScope(settings.PinnedDirectory);
            player.SetFilter(filterType);

            await player.PlayRandom();

            if (playerCommand is not null)
            {
                playerCommand.Execute(context, new());
            }
            return 0;
        }
    }
}