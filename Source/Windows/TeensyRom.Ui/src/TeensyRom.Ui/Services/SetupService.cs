using ReactiveUI;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Features.Common.State.Player;
using TeensyRom.Ui.Features.Discover.State;
using TeensyRom.Ui.Features.NavigationHost;

namespace TeensyRom.Ui.Services
{
    public interface ISetupService
    {
        Task StartSetup();

    }
    public class SetupService : ISetupService
    {
        private readonly ISettingsService _settingsService;
        private readonly INavigationService _navigation;
        private readonly ISerialStateContext _serial;
        private readonly IDialogService _dialog;
        private readonly ICachedStorageService _storage;
        private readonly IDiscoverContext _discover;
        private TeensySettings _settings = null!;

        public SetupService(ISettingsService settingsService, INavigationService navigation, ISerialStateContext serial, IDialogService dialog, ICachedStorageService storage, IDiscoverContext discover)
        {
            _settingsService = settingsService;
            _navigation = navigation;
            _serial = serial;
            _dialog = dialog;
            _storage = storage;
            _discover = discover;
            _settingsService.Settings.Subscribe(settings => _settings = settings);
        }
        public async Task StartSetup()
        {
            _settings = _settingsService.GetSettings();

            if (!_settings.FirstTimeSetup) return;

            var result = await _dialog.ShowConfirmation("Welcome to TeensyROM!", "This start up guide will help you get set up and learn about a few basic features of the app. \r\rAt any point, feel free to press cancel to finish up on your own.");

            if (!result)
            {
                await Complete();
                return;
            }
            await OnConnectable();
        }

        public async Task OnConnectable()
        {
            _serial.CurrentState
                .Where(s => s is SerialConnectableState)
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async _ =>
                {
                    var result = await _dialog.ShowConfirmation("Connect to the TR", "Click on the Connect button.  If things go ok, we'll continue to the next step. \r\rNote, your TR will reset once you connect.  This is normal.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    OnConnected();
                });

            var currentSerialState = await _serial.CurrentState.FirstAsync();

            var result = false;

            if (currentSerialState is not SerialConnectableState)
            {
                result = await _dialog.ShowConfirmation("No COM Ports Detected", "Things to try before we proceed: \r\r· Make sure your USB cable is seated properly \r· Try a different USB cable \r· Make sure the C64 is turned on or try restarting it \r\rOnce I detect COM ports, we'll go to the next step.");
            }
        }

        public void OnConnected()
        {
            _serial.CurrentState
                .Where(s => s is SerialConnectedState)
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler).Subscribe(async _ =>
                {
                    var result = await _dialog.ShowConfirmation("Successful Connection!", "Let's head over to the settings view and get some things configured.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    _navigation.NavigateTo(NavigationLocation.Settings);

                    result = await _dialog.ShowConfirmation("Select Storage Device", "Here, you're going to select your preferred storage device.  \r\rFor now, the app only supports a single storage device at a time.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Automatic File Transfer", "Another feature you can configure here is the \"Watch Directory\".  Any files you place into this directory will automatically get copied to the TR.  \r\rI've set the directory of your default downloads folder.  If you download a SID, CRT, PRG or HEX firmware with your web browser, you should automatically see it in the /auto-transfer folder. \r\rFeel free to change this watch folder or disable the feature.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Save Your Settings", "Once you're done configuring your settings, click the \"Save Settings\" button and we'll go to the next step.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    OnSettingsSaved();
                });
        }

        public void OnSettingsSaved()
        {
            _settingsService.Settings
                .Skip(1)
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async settings =>
            {
                var result = await _dialog.ShowConfirmation("Next, we're going to index all the file locations on your selected storage device.\r\r This will increase performance, stability, and unlock some fun search and randomization features.");

                if (!result)
                {
                    await Complete();
                    return;
                }

                result = await _dialog.ShowConfirmation("Copy Some Files!", "Copy some files now before we start the indexing process.  \r\rTotally optional, but strongly recommended, consider copying HVSC and OneLoad64 onto your SD or USB storage. You can always do this later if you want.\r\r Clicking \"OK\" will start the indexing process.");

                if (!result)
                {
                    await Complete();
                    return;
                }

                await OnCache();
            });
        }

        public async Task OnCache()
        {
            var cachingTask = _storage.CacheAll();
            var result = await _dialog.ShowConfirmation("Let's navigate back to the connection screen.  You can watch as all the files are read from the TR.\r\rHang tight as this will likely take several minutes if you have thousands of files.");
            _navigation.NavigateTo(NavigationLocation.Connect);

            _serial.CurrentState
                .Where(s => s is SerialConnectedState)
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler).Subscribe(async _ =>
                {
                    var result = await _dialog.ShowConfirmation("File Indexing Completed", $"Now that your {_settings.TargetType} file information has been indexed, lets head over to the Discover view and do some exploring.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    _navigation.NavigateTo(NavigationLocation.Discover);

                    result = await _dialog.ShowConfirmation("Indexing Files", "If you make any changes to your storage outside of this application, you can always re-index all your files by clicking on the download button in the upper right.  \r\rNote, if you avoid indexing all your files, your random play and search capabilities will be limited to the folders you have visited.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Discovery View", $"In the \"Discover\" view, you can navigate and launch music and games (or other programs) on your {_settings.TargetType} storage.  \r\rIn the first 2 sections, you should see the root directory structure and file listing of your {_settings.TargetType} storage.\r\rOn the right you will find some file information for the currently selected file.  If you copy the HVSC and OneLoad64 collection to your SD, you will be treated with some extra content here.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Transfer Files", "You can drag and drop files or folders onto the file listing of the current directory to transfer files to the TR. \r\rWarning, if you drag a folder that has other nested folders, they will all be copied!");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Feeling Lucky?", "Let's try discovering something to play. \r\rClick on the die button near the lower left of the screen next to the \"All\", \"Games\", and \"Music\" filters.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    OnLaunch();
                });
        }

        public void OnLaunch()
        {
            _discover.LaunchedFile
                .OfType<ILaunchableItem>()
                .Where(file => file.IsCompatible is true)
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async file =>
                {
                    var itemType = file is SongItem ? "SID" : "Game";

                    var result = await _dialog.ShowConfirmation("Random Launch", $"I see you discoverd a {itemType} called \"{file.Name}\", nice! \r\rNotice the \"All\" filter is selected in the lower left.  Currently, either Games or Music will be launched randomly.  \r\rTry selecting the \"Games\" or \"Music\" filter.  After, try clicking the \"Next\" button on the play toolbar.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    OnLaunchGame();
                });
        }
        public void OnLaunchGame()
        {
            _discover.LaunchedFile
                .OfType<ILaunchableItem>()
                .Skip(1)
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async file =>
                {
                    var itemType = string.Empty;

                    var result = false;

                    if (file is GameItem)
                    {
                        result = await _dialog.ShowConfirmation("Random Game Launched", $"Nice, you found a game located at {file.Path}.  \r\rNotice how the directory listing has also changed to location of the launched file.");
                    }
                    else if (file is SongItem)
                    {
                        result = await _dialog.ShowConfirmation("Random Music Launched", $"Nice, you found a SID located at {file.Path}.  \r\rNotice how the directory listing has also changed to location of the launched file.");
                    }

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Play Toolbar", "As you may have noticed, the \"Discover\" view functions like a media player.  Depending on the mode you're in and the filter you have selected, clicking \"Previous\" or \"Next\" will have a different behavior.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Play Toolbar", "Currently we're in \"Shuffle Mode\" as indicated by the blue crossed arrows on the right.  As you saw, the \"Next\" button played the next random file.  \r\rNote, in this application, shuffle mode works across your entire collection, not just the current directory.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Play Toolbar", "Try clicking the \"Shuffle Mode\" button to turn it off.  Then click the \"Next\" button.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    OnNormalPlay();
                });
        }

        public void OnNormalPlay()
        {
            _discover.LaunchedFile
                .OfType<ILaunchableItem>()
                .Skip(1)
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async file =>
                {
                    var itemType = string.Empty;

                    var result = await _dialog.ShowConfirmation("Normal Play Mode", $"The next file in the current directory should have launched if you turned off shuffle mode.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Music Only", $"Music has some special behaviors that games do not.  \r\r· Music will go to the next SID automatically if it ends. \r· The previous button will restart the current SID.  \r· Quickly clicking a second time will go to the previous SID.  \r· A share button allows you to share DeepSID links with your friends.\r\rHave you installed the HVSC yet? ;)");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Games Only", $"Games also have some unique behaviors.  \r\r· Games will have a \"Stop\" button instead of \"Pause\".  \r· Stop will reset the TR\r· Tagging a favorite while a game is playing will re-launch the game.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Favorite Your Favorites!", $"Whenever you see a \"Heart\" button throughout the application, when you click it, the file will be saved to the /favorites folder.  \r\rThis will physically copy the file there, so you can find the favorites while using TR C64 UI directly as well.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Search", $"In the upper right, you can type multiple keywords to search for games or SIDs.  As you would expect, \"All\", \"Games\" and \"Music\" will filter your search results.\r\rPlaying Next or Previous will move through the files in your search results.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("", $"I hope these tools help you have fun finding new content!  \r\rLet's head over to the \"Help\" section now.");

                    await Complete();
                });
        }

        public Task Complete()
        {
            var settings = _settingsService.GetSettings();
            settings.FirstTimeSetup = false;
            _settingsService.SaveSettings(settings);

            _navigation.NavigateTo(NavigationLocation.Help);

            return _dialog.ShowConfirmation("Setup Wizard Complete", "The help section is a great place to find out more about the project.\r\rVisit the GitHub project page for more detailed documentation on the application features or troubleshooting tips.\r\r  If you'd like to re-run this tutorial, you can do so from here.\r\rEnjoy! ;)");
        }
    }
}
