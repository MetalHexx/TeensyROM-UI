using Spectre.Console;
using Spectre.Console.Cli;
using TeensyRom.Cli.Helpers;
using TeensyRom.Core.Settings;

namespace TeensyRom.Cli.Commands.Main.Launcher
{
    internal class LaunchSettings : CommandSettings, IClearableSettings, IRequiresConnection
    {
        public void ClearSettings() { }

        public static string Description => "Launch files from your storage device.";
        public static string Example => "launch";
    }
    internal class LaunchCommand : AsyncCommand<LaunchSettings>
    {
        private readonly RandomCommand _randomCommand;
        private readonly SearchCommand _searchCommand;
        private readonly NavigateCommand _navigateCommand;
        private readonly FileLaunchCommand _fileCommand;
        private readonly PlayerCommand _playerCommand;
        private readonly IndexCommand _indexCommand;
        private readonly ISettingsService _settingsService;

        public LaunchCommand(ITypeResolver resolver, ISettingsService settingsService)
        {
            _randomCommand = (resolver.Resolve(typeof(RandomCommand)) as RandomCommand)!;
            _searchCommand = (resolver.Resolve(typeof(SearchCommand)) as SearchCommand)!;
            _navigateCommand = (resolver.Resolve(typeof(NavigateCommand)) as NavigateCommand)!;
            _fileCommand = (resolver.Resolve(typeof(FileLaunchCommand)) as FileLaunchCommand)!;
            _playerCommand = (resolver.Resolve(typeof(PlayerCommand)) as PlayerCommand)!;
            _indexCommand = (resolver.Resolve(typeof(IndexCommand)) as IndexCommand)!;

            if (_randomCommand == null || _navigateCommand == null || _fileCommand == null || _playerCommand == null)
            {
                throw new Exception("Failed to resolve command dependencies");
            }

            _settingsService = settingsService;
        }
        public override async Task<int> ExecuteAsync(CommandContext context, LaunchSettings settings)
        {
            var globalSettings = _settingsService.GetSettings();

            if (!globalSettings.HasIndexed) 
            {
                RadHelper.WriteMenu("Index Missing", "You have not yet indexed the storage on your TeensyROM. Indexing your files is important for search and random launch.  If you avoid indexing your files, only directories you have navigated to and launched files from will be available.");
                var shouldIndex = PromptHelper.ChoicePrompt("Index Files", ["Yes", "No"]);

                if(shouldIndex == "Yes")
                {
                    await _indexCommand.ExecuteAsync(context, new IndexSettings());
                }
            }

            var shouldLeave = false;

            do
            {
                RadHelper.WriteMenu("Start a stream", "Consider a play timer to stream SIDs, Demos, and Game Intros together. Retro Jukebox! ;)");

                RadHelper.WriteDynamicTable(["Menu Item", "Description"],
                [
                    ["Random", "Play a random file."],
                    ["Search", "Search for files."],
                    ["Navigate", "Navigate storage to find a file."],
                    ["File", "Launch a file directory with a path."],
                    ["Player", "Go to the player view."],
                    ["Back", "Back to main menu."],
                ]);

                switch (PromptHelper.ChoicePrompt("Choose wisely", ["Random", "Search", "Navigate", "File", "Player", "Back"]))
                {
                    case "Random":
                        await _randomCommand.ExecuteAsync(context, new RandomSettings());
                        break;

                    case "Search":
                        await _searchCommand.ExecuteAsync(context, new SearchSettings());
                        break;

                    case "Navigate":
                        await _navigateCommand.ExecuteAsync(context, new NavigateSettings());
                        break;

                    case "File":
                        await _fileCommand.ExecuteAsync(context, new FileLaunchSettings());
                        break;

                    case "Player":
                        _playerCommand.Execute(context, new PlayerSettings());
                        break;

                    default:
                        shouldLeave = true;
                        break;
                }
            } while (!shouldLeave);

            AnsiConsole.WriteLine();
            return 0;
        }
    }
}
