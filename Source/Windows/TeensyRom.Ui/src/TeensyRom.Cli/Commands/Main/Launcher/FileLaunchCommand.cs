using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using TeensyRom.Cli.Helpers;
using TeensyRom.Cli.Services;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Common;
using TeensyRom.Core.Settings;

namespace TeensyRom.Cli.Commands.Main.Launcher
{
    internal class FileLaunchSettings : LaunchSettings, IClearableSettings, IRequiresConnection
    {
        [Description("Storage device of file to launch. (sd or usb)")]
        [CommandOption("-s|--storage")]
        public string StorageDevice { get; set; } = string.Empty;

        [Description("The full path of the file to launch.")]
        [CommandOption("-p|--path")]
        public string FilePath { get; set; } = string.Empty;

        public static string Example => "launch file -s sd -p /music/MUSICIANS/T/Tjelta_Geir/Artillery.sid";
        public static string Description => "Launch a specific file.";

        public new void ClearSettings()
        {
            StorageDevice = string.Empty;
            FilePath = string.Empty;
        }

        public override ValidationResult Validate()
        {
            if (!StorageDevice.Equals(string.Empty) && !StorageDevice.IsValueValid(["sd", "usb"]))
            {
                return ValidationResult.Error($"Storage device must be 'sd' or 'usb'.");
            }
            if (!FilePath.Equals(string.Empty) && !FilePath.IsValidUnixPath())
            {
                return ValidationResult.Error($"Must be a valid unix path.");
            }
            return base.Validate();
        }
    }

    internal class FileLaunchCommand(IPlayerService player, ICachedStorageService storage, ISettingsService settingsService, ITypeResolver resolver) : AsyncCommand<FileLaunchSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, FileLaunchSettings settings)
        {
            player.StopStream();

            RadHelper.WriteMenu("Launch File", "Launch a specific file.",
            [
               "When playing a SID, or you have a Game/Image timer enabled, the next file in the directory will be played.",
               "Parent directory will be indexed on first visit.",
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
            settings.FilePath = CommandHelper.PromptForFilePath(settings.FilePath);
            await player.SetDirectoryMode(settings.FilePath);

            if (!settings.ValidateSettings()) return -1;
            
            await player.LaunchItem(settings.FilePath);

            var playerCommand = resolver.Resolve(typeof(PlayerCommand)) as PlayerCommand;

            if (playerCommand is not null)
            {
                playerCommand.Execute(context, new());
            }
            return 0;
        }
    }
}