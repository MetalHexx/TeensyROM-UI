using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Reactive.Linq;
using TeensyRom.Cli.Helpers;
using TeensyRom.Cli.Services;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Settings;

namespace TeensyRom.Cli.Commands.Main.Launcher
{
    internal class NavigateSettings : LaunchSettings, IClearableSettings, IRequiresConnection
    {
        [Description("Storage device of file to launch. (sd or usb)")]
        [CommandOption("-s|--storage")]
        public string StorageDevice { get; set; } = string.Empty;

        [Description("The path of the files to list.")]
        [CommandOption("-p|--path")]
        [DefaultValue("/")]
        public string StartingPath { get; set; } = "/";

        public new static string Example => "launch nav -s SD -p /music/MUSICIANS/T/Tjelta_Geir/";
        public new static string Description => "Navigate through the storage directories and pick a file to launch.";

        public new void ClearSettings()
        {
            StorageDevice = string.Empty;
        }

        public override ValidationResult Validate()
        {
            if (!StorageDevice.Equals(string.Empty) && !StorageDevice.IsValueValid(["sd", "usb"]))
            {
                return ValidationResult.Error($"Storage device must be 'sd' or 'usb'.");
            }
            if (!StartingPath.Equals(string.Empty) && !StartingPath.IsValidUnixPath())
            {
                return ValidationResult.Error($"Must be a valid unix path.");
            }
            return base.Validate();
        }
    }

    internal class NavigateCommand(ICachedStorageService storage, ITypeResolver resolver, IPlayerService player, ISettingsService settingsService) : AsyncCommand<NavigateSettings>
    {
        private string _lastDirectory = string.Empty;

        public override async Task<int> ExecuteAsync(CommandContext context, NavigateSettings settings)
        {
            var playerCommand = resolver.Resolve(typeof(PlayerCommand)) as PlayerCommand;

            player.StopStream();

            RadHelper.WriteMenu("Navigate Storage", "Navigate through the storage directories and pick a file to launch.",
            [
               "Your player mode will be set to \"Directory\" when you launch from here.",
               "Your last visited directory will remembered for next time."
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
            if (string.IsNullOrWhiteSpace(_lastDirectory)) _lastDirectory = settings.StartingPath;

            var cacheItem = await storage.GetDirectory(_lastDirectory);

            if (cacheItem is null || !cacheItem.Files.Any() && !cacheItem.Directories.Any())
            {
                RadHelper.WriteError($"No directories or files found in {storageType.ToString()} at path {_lastDirectory}");
                AnsiConsole.WriteLine();
                return 0;
            }

            var directories = PrepareDirectories(cacheItem.Directories);

            List<StorageItem> storageItems = directories.Concat(cacheItem.Files.Cast<StorageItem>()).ToList();

            var selectedFile = await TraverseStorage(storageItems, _lastDirectory);


            if (selectedFile is null)
            {
                RadHelper.WriteError("Directory or files not found.");
                AnsiConsole.WriteLine();
                return 0;
            }
            AnsiConsole.WriteLine();

            player.SetDirectoryMode(selectedFile.Path);
            player.SetStorage(storageType);
            await player.LaunchFromDirectory(selectedFile.Path);

            if (playerCommand is not null)
            {
                playerCommand.Execute(context, new());
            }
            return 0;
        }

        public async Task<FileItem?> TraverseStorage(List<StorageItem> items, string targetDirectory)
        {
            if (targetDirectory != "/")
            {
                items.Insert(0, new StorageItem 
                {
                    Name = "..",
                    Path = targetDirectory.GetUnixParentPath(),
                });                
            }
            var storageItemName = PromptHelper.FilePrompt(targetDirectory, items.Select(s => s.Name).ToList());

            var selectedStorageItem = items.First(s => s.Name == storageItemName);

            if (selectedStorageItem is FileItem file)
            {
                _lastDirectory = file.Path.GetUnixParentPath();
                return file;
            }
            var nextDirectory = selectedStorageItem.Path;

            var cacheItem = await storage.GetDirectory(nextDirectory);

            if (cacheItem is null)
            {
                RadHelper.WriteError("Directory not found.");
                AnsiConsole.WriteLine();
                return null;
            }
            List<DirectoryItem> directories = PrepareDirectories(cacheItem.Directories);

            List<StorageItem> storageItems = directories.Concat(cacheItem.Files.Cast<StorageItem>()).ToList();

            return await TraverseStorage(storageItems, nextDirectory);
        }

        public List<DirectoryItem> PrepareDirectories(List<DirectoryItem> directories)
        {
            return directories                
                .Select(d => d.Clone() as DirectoryItem)
                .Select(d =>
                {
                    d.Name = $"/{d.Name}";
                    return d;
                })
                .ToList();
        }
    }
}