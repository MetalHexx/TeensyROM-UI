using Spectre.Console;
using Spectre.Console.Cli;
using TeensyRom.Cli.Helpers;
using TeensyRom.Cli.Services;
using TeensyRom.Cli.Core.Player;
using TeensyRom.Cli.Core.Storage.Services;
using TextCopy;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Cli.Commands.Main.Launcher
{
    internal class PlayerSettings : LaunchSettings, IClearableSettings, IRequiresConnection
    {
        public new void ClearSettings() { }
    }

    internal class PlayerCommand : Command<PlayerSettings>
    {
        private readonly IPlayerService _player;
        private readonly ICachedStorageService _storage;
        private PlayState? _previousState;

        public PlayerCommand(IPlayerService player, ICachedStorageService storage)
        {
            _player = player;
            _storage = storage;
            _player.FileLaunched.Subscribe(DisplayLaunchedFile);
        }
        public override int Execute(CommandContext context, PlayerSettings settings)
        {
            RadHelper.WriteMenu("Stream Player", "There are many paths your stream can take...");

            var choice = string.Empty;

            do
            {
                var _playerState = _player.GetState();

                List<string> choices = _playerState.PlayState == PlayState.Playing
                    ? ["Next", "Previous", "Stop/Pause", "Favorite", "Ban", "Share", "Stream Settings", "Back"]
                    : ["Next", "Previous", "Resume", "Favorite", "Ban", "Share", "Stream Settings", "Back"];

                choice = PromptHelper.ChoicePrompt("Choose wisely", choices);

                switch (choice)
                {
                    case "Next":
                        _player.PlayNext();
                        break;

                    case "Previous":
                        _player.PlayPrevious();
                        break;

                    case "Resume":
                        _player.TogglePlay();
                        break;

                    case "Stop/Pause":
                        _player.TogglePlay();
                        break;

                    case "Favorite":
                        HandleFavorite(_playerState);
                        break;

                    case "Ban":
                        HandleBan(_playerState);
                        break;

                    case "Share":
                        HandleShare(_playerState);
                        break;

                    case "Stream Settings":                        
                        HandleSettings();
                        break;
                }

            } while (choice != "Back");

            return 0;
        }

        private void HandleSettings() 
        {
            AnsiConsole.WriteLine();

            var choice = string.Empty;

            RadHelper.WriteMenu("Stream Settings", "Try changing some settings to alter the behavior of the stream.",
            [
                "Many games have nice intro music with visuals.",
                "Multimedia Stream: Set a play timer to stream games, demos and SIDs and discover something new.",
                "Demo Stream:  Set \"Pinned Directory\" to a folder with demos and add a timer.",
                "Favorite Stream: Set \"Pinned Directory\" to \\favorites.",
                "Screensaver: Set \"Filter\" to Images with a timer."
            ]);

            do
            {
                WriteHelp();
                var playerState = _player.GetState();

                List<string> choices = ["Mode", "Filter", "Timer", "Pin Directory", "Help", "Back"];

                choice = PromptHelper.ChoicePrompt("Choose wisely", choices);

                switch (choice)
                {
                    case "Mode":
                        HandleMode(playerState);
                        break;

                    case "Filter":
                        HandleFilter();
                        break;

                    case "Pin Directory":
                        HandlePinDirectory();
                        break;

                    case "Timer":
                        HandleTimer();
                        break;

                    case "Help":
                        WriteHelp();
                        break;
                }

            } while (choice != "Back");
        }

        private static void HandleShare(PlayerState playerSettings)
        {
            if (playerSettings.CurrentItem is not null)
            {
                RadHelper.WriteLine();
                ClipboardService.SetText(playerSettings.CurrentItem.ShareUrl);
                RadHelper.WriteLine("A DeepSID share link has been copied to your clipboard.");
                RadHelper.WriteLine();
            }
        }

        private void HandleTimer()
        {
            var timer = CommandHelper.PromptGameTimer("");
            var sidTimerSelection = CommandHelper.PromptSidTimer("");
            _player.SetSidTimer(sidTimerSelection);
            _player.SetStreamTime(timer);
        }

        private void HandlePinDirectory()
        {
            var path = CommandHelper.PromptForDirectoryPath("");

            if (!path.IsValidUnixPath())
            {
                RadHelper.WriteError("Not a valid Unix path");
                return;
            }
            _player.SetDirectoryScope(path);
        }

        private void HandleFilter()
        {
            var filter = CommandHelper.PromptForFilterType("");
            _player.SetFilter(filter);
        }

        private void HandleMode(PlayerState playerState)
        {
            var playMode = PromptHelper.ChoicePrompt("Play Mode", ["Random", "Current Directory"]);

            if (playMode == "Random")
            {
                _player.SetRandomMode(playerState.ScopePath);
                return;
            }
            var directoryPath = playerState.CurrentItem is null
                ? "/"
                : playerState.CurrentItem.Path.GetUnixParentPath();

            _player.SetDirectoryMode(directoryPath);
        }

        private void HandleFavorite(PlayerState playerState)
        {
            AnsiConsole.WriteLine(RadHelper.ClearHack);

            var needsToggle = playerState.PlayState is PlayState.Playing;

            if (playerState.CurrentItem is null)
            {
                RadHelper.WriteLine("No file is currently playing");
                return;
            }
            var shouldRemove = false;

            if(playerState.CurrentItem.IsFavorite)
            {
                shouldRemove = PromptHelper.Confirm("Remove Favorite?", false);

                if (!shouldRemove) return;
            }

            if (needsToggle) _player.TogglePlay();

            if (shouldRemove)
            {
                _storage.RemoveFavorite(playerState.CurrentItem);
            }
            else 
            {
                _storage.SaveFavorite(playerState.CurrentItem);
            }
            if (needsToggle) _player.TogglePlay();
        }

        private void HandleBan(PlayerState playerState)
        {
            AnsiConsole.WriteLine(RadHelper.ClearHack);

            var needsToggle = playerState.PlayState is PlayState.Playing;

            if (playerState.CurrentItem is null)
            {
                RadHelper.WriteLine("No file is currently playing");
                return;
            }

            var shouldBan = PromptHelper.Confirm($"Are you sure you want to ban {playerState.CurrentItem.Name}?", false);

            if (!shouldBan) return;

            if (needsToggle) _player.TogglePlay();

            if (shouldBan)
            {
                _storage.BanFile(playerState.CurrentItem);
                _player.PlayNext();
            }
        }

        private void WriteHelp()
        {
            var playerSettings = _player.GetState();

            var mode = playerSettings.PlayMode switch
            {
                PlayMode.CurrentDirectory => "Current Directory",
                PlayMode.Random => "Random",
                PlayMode.Search => "Search Results",
                _ => "Random"
            };

            var sidTimer = playerSettings.SidTimer is SidTimer.SongLength
                ? "Song Length"
                : "Timer Override";

            //AnsiConsole.WriteLine();
            RadHelper.WriteDynamicTable(["Setting", "Current Value", "Description"],
            [
                ["Storage Device", playerSettings.StorageType.ToString(), "The storage device the file is stored on."],
                ["Current Directory", playerSettings.CurrentItem?.Path.GetUnixParentPath() ?? "---", "Directory of the playing file."],
                ["File", playerSettings.CurrentItem?.Name ?? "---", "The current file playing."],
                ["Mode", mode, "Play random or stick to a specific directory."],                
                ["Filter", playerSettings.FilterType.ToString(), "The types of files that will be played."],
                ["Timer", playerSettings.PlayTimer?.ToString() ?? "---", "Continuous play timer for Games, Images and SIDs." ],
                ["SID Timer", sidTimer, "SIDs play time based on song length or overriden w/timer."],
                ["Pinned Directory", playerSettings.ScopePath, "Random mode will launch from this directory and subdirs."],
                ["Search Query", playerSettings.SearchQuery ?? "---", "The current search query your  is using."],     
            ]);
        }

        private void DisplayLaunchedFile(ILaunchableItem item) 
        {
            AnsiConsole.WriteLine(RadHelper.ClearHack);
            var release = string.IsNullOrWhiteSpace(item.ReleaseInfo) ? "Unknown" : item.ReleaseInfo.EscapeBrackets();

            var body = string.Empty;

            if (item is SongItem song)
            {
                body = $"\r\nCreator: {song.Creator}\r\nRelease: {release}\r\nLength: {song.PlayLength}\r\nClock: {song.Meta1}\r\nSID: {song.Meta2}";
            }
            var isFavorite = item.IsFavorite ? "Yes" : "No";

            body = $"{body}\r\nPath: {item.Path.EscapeBrackets()}\r\nFavorite: {isFavorite}\r\n";

            var fileInfoPanel = new Panel(body.EscapeBrackets())
                .Header($" Now Playing: {item.Title.EscapeBrackets()} ".AddHighlights())
                .PadLeft(3)
                .BorderColor(RadHelper.Theme.Secondary.Color)
                .Border(BoxBorder.Rounded)
                .Expand();

            AnsiConsole.Write(fileInfoPanel);

            if (item is SongItem && !string.IsNullOrWhiteSpace(item.Description))
            {
                var stilCommentPanel = new Panel($"\r\n{item.Description.EscapeBrackets().Trim()}\r\n")
                .Header(" SID Comments ".AddSecondaryColor())
                .PadLeft(3)
                .BorderColor(RadHelper.Theme.Primary.Color)
                .Border(BoxBorder.Rounded)
                .Expand();

                AnsiConsole.Write(stilCommentPanel);
            }            
        }
    }
}
