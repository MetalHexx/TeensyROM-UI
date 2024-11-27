using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Serial.State;
using TeensyRom.Ui.Core.Storage.Services;
using TeensyRom.Ui.Features.Discover;
using TeensyRom.Ui.Features.Discover.State.Player;
using TeensyRom.Ui.Features.NavigationHost;

namespace TeensyRom.Ui.Services
{
    public interface ISetupService
    {
        void ResetSetup();
        Task StartSetup();

    }
    public class SetupService : ISetupService
    {
        private readonly ISettingsService _settingsService;
        private readonly INavigationService _navigation;
        private readonly ISerialStateContext _serial;
        private readonly IDialogService _dialog;
        private readonly IAlertService _alert;
        private readonly ICachedStorageService _storage;
        private readonly IPlayerContext _player;
        private readonly DiscoverViewModel _discoverView;
        private TeensySettings _settings = null!;
        private bool _sdSuccess = false;
        private bool _usbSuccess = false;

        public SetupService(ISettingsService settingsService, INavigationService navigation, ISerialStateContext serial, IDialogService dialog, IAlertService alert, ICachedStorageService storage, IPlayerContext player, DiscoverViewModel discoverView)
        {
            _settingsService = settingsService;
            _navigation = navigation;
            _serial = serial;
            _dialog = dialog;
            _alert = alert;
            _storage = storage;
            _player = player;
            _discoverView = discoverView;
            _settingsService.Settings.Subscribe(settings => _settings = settings);
        }
        public void ResetSetup()
        {
            _settings = _settingsService.GetSettings();
            _settings.FirstTimeSetup = true;
            _settingsService.SaveSettings(_settings);
        }
        public async Task StartSetup()
        {
            _settings = _settingsService.GetSettings();
            var connected = await _serial.CurrentState.Select(s => s is SerialConnectedState).FirstAsync();

            if (!_settings.FirstTimeSetup) return;

            if (connected) 
            {
                _alert.Publish("Disconnecting from TR to run first time setup wizard.");
                _serial.ClosePort();
            }

            var currentView = await _navigation.SelectedNavigationView.FirstOrDefaultAsync();

            if (currentView.Type is not NavigationLocation.Terminal)
            {
                _navigation.NavigateTo(NavigationLocation.Terminal);
            }

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
                    var result = await _dialog.ShowConfirmation("Successful Connection!", "You have connected to the TR successfully.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Connection View", "On this screen, you will find a few utility features that might come in handy.\r\r· Ping: This is useful for troubleshooting connectivity. \r· Reset: Will reset the C64. \r· Logs: Useful for troubleshooting or debugging. \r· CLI: Will send serial commands to the TR");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Settings", "Let's head over to the settings view and get your preferences configured.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    _navigation.NavigateTo(NavigationLocation.Settings);

                    result = await _dialog.ShowConfirmation("Settings", "There are various settings here to customize the application behavior to your preferences.  \r\rMouse over the different settings to see a tooltip for what they do.\r\r Note: Help tooltips are present on most controls in the app. ");

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
                var result = await _dialog.ShowConfirmation("Let's Index", "Next, we're going to index all the file locations on your storage devices.\r\rThe indexing process is vital for getting the best user experience with the application.");

                if (!result)
                {
                    await Complete();
                    return;
                }

                result = await _dialog.ShowConfirmation("Copy Some Files!", $"Copy some files to your SD or USB storage now before we start the indexing process.  \r\rTotally optional, but strongly recommended, consider copying HVSC and OneLoad64 onto your {_settings.StorageType} storage. You can always do this later if you want.\r\rClicking \"OK\" will start the indexing process.");

                if (!result)
                {
                    await Complete();
                    return;
                }

                await OnCacheSD();
            });
        }

        public async Task OnCacheSD()
        {   
            var result = await _dialog.ShowConfirmation("Let's navigate back to the connection screen.  You can watch as all the files are read from the TR.\r\rHang tight as this will likely take several minutes if you have thousands of files.");
            
            _navigation.NavigateTo(NavigationLocation.Terminal);

            _dialog.ShowNoClose("Indexing SD Storage", "This may a few minutes.  Don't touch your commodore while indexing is in progress.");

            _settings.StorageType = TeensyStorageType.SD;
            _settingsService.SaveSettings(_settings);
            await _storage.CacheAll(StorageConstants.Remote_Path_Root);
            _dialog.HideNoClose();

            await OnCacheUSB();
        }

        public async Task OnCacheUSB()
        {
            _dialog.ShowNoClose("Indexing USB Storage", "This may take a few minutes.  Don't touch your commodore while indexing is in progress.");
            _settings.StorageType = TeensyStorageType.USB;
            _settingsService.SaveSettings(_settings);
            await _storage.CacheAll(StorageConstants.Remote_Path_Root);
            _dialog.HideNoClose();

            var result = await _dialog.ShowConfirmation("File Indexing Completed", $"Now that your file information has been indexed, lets head over to the Discover view and do some exploring.");

            if (!result)
            {
                await Complete();
                return;
            }

            await OnDiscover();
        }

        public async Task OnDiscover() 
        {
            _navigation.NavigateTo(NavigationLocation.Discover);

            _discoverView.StorageSelector.SelectedStorage = TeensyStorageType.SD;

            var result = await _dialog.ShowConfirmation("Discovery View", $"In the \"Discover\" view, you can navigate and launch music, games or images.  \r\rIn the first 2 sections, you should see the root directory structure and file listing of your {_settings.StorageType} storage.");

            if (!result)
            {
                await Complete();
                return;
            }

            result = await _dialog.ShowConfirmation("Storage", $"{_settings.StorageType} storage is currently selected.  You can switch between SD and USB storage using the dropdown next to the \"Discover\" title.");

            if (!result)
            {
                await Complete();
                return;
            }

            result = await _dialog.ShowConfirmation("Re-indexing Storage", $"If you make changes to your selected storage outside of this application, you can re-index it clicking on the down arrow button next to the storage dropdown.");

            if (!result)
            {
                await Complete();
                return;
            }

            result = await _dialog.ShowConfirmation("Feeling Lucky?", "Let's try discovering something to play. \r\rClick on the die button near the lower left of the screen next to the \"All\", \"Games\", \"Music\" and \"Images\" filters.");

            if (!result)
            {
                await Complete();
                return;
            }
            OnLaunch();
        }

        public void OnLaunch()
        {
            _player.LaunchedFile
                .Where(f => f?.File is not null)
                .Select(f => f.File)
                .OfType<ILaunchableItem>()
                .Where(file => file.IsCompatible is true)
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async file =>
                {
                    var itemType = file switch
                    {
                        SongItem => "SID",
                        GameItem => "Game",
                        ImageItem => "Image",
                        _ => "File"
                    };

                    var result = await _dialog.ShowConfirmation("Random Launch", $"I see you discoverd a {itemType} located at {file.Path}, nice! \r\rNotice the \"All\" filter is selected in the lower left.  This means any file type (Games, Music, or Images) will be launched randomly when your roll the dice.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Filters", $"Try selecting the \"Games\" or \"Music\" filter.  You will notice that pressing the filter buttons will also automatically trigger a random launch for the selected file type.");

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
            _player.LaunchedFile
                .Where(f => f?.File is not null)
                .Select(f => f.File)
                .OfType<ILaunchableItem>()
                .Skip(1)
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async file =>
                {
                    var itemType = file switch
                    {
                        SongItem => "SID",
                        GameItem => "Game",
                        ImageItem => "Image",
                        _ => "File"
                    };

                    var result = await _dialog.ShowConfirmation("Random Launch", $"Nice, you found a {itemType} located at {file.Path}.");

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
                    result = await _dialog.ShowConfirmation("Shuffle Mode", "Currently we're in \"Shuffle Mode\" as indicated by the blue crossed arrows on the right.  \r\rThis means clicking the next button will launch a random file from your collection depending on the filter you have selected.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Shuffle Mode", "History is kept while in shuffle mode.  So if you click the previous button, you will launch the previous file.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Play Toolbar", "Try clicking the \"Shuffle Mode\" button to disable it.  Then click the \"Next\" button.");

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
            _player.LaunchedFile
                .Where(f => f?.File is not null)
                .Select(f => f.File)
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
                    result = await _dialog.ShowConfirmation("Continuous Play", $"All file types support continuous play.  \r\r·SIDs will automatically go to the next track when it ends. \r·Games and Images have an optional play timer you can enable.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Play Timer", $"Try clicking the \"Games\" or \"Images\" filter.  You will notice a \"Stopwatch\" icon in the \"Play\" toolbar.\r\rClicking the \"Stopwatch\" button will enable continuous play on games and images.  You can select a play time length that best suits your needs.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Tag Your Favorites!", $"Whenever you see a \"Heart\" button throughout the application, when you click it, the file will be saved to the /favorites folder.  \r\rSince the tagged favorite is a copy of the file, you can launch it directly from the Commodore UI as well.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Search", $"In the upper right, you can type multiple keywords to search for games or SIDs.\r\r The \"All\", \"Games\", \"Music\" and \"Images\" buttons will filter your search results.\r\rPlaying Next or Previous will move through the files in your search results.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Search Phrases", $"In addition to keywords, you can also group keywords together in double quotes to search for a phrase.\r\r Ex: \"Iron Maiden\" Aces High");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Search Required Terms", $"Search terms are optional, so not all terms must be present in a search result. If a search result MUST have a specific keyword or phrase, you can place a + in front of it\r\r Ex: +\"Iron Maiden\" Aces +High\r\rIn this query, the term \"Aces\" will not be required but \"Iron Maiden\" and \"High\" will be.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Search Filtering", $"Note: filters will not trigger a file launch when viewing search results.  Instead, search results will be filtered by the selected file type.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("", $"I hope these tools help you have fun finding new content!  \r\rThere are other features you can learn about in the docs or tooltips throughout the app.  \r\rLet's head over to the \"Help\" section now.");

                    await Complete();
                });
        }

        public Task Complete()
        {
            var settings = _settingsService.GetSettings();
            settings.FirstTimeSetup = false;
            _settingsService.SaveSettings(settings);

            _navigation.NavigateTo(NavigationLocation.Help);

            return _dialog.ShowConfirmation("Setup Wizard Complete", "The help section is a great place to find out more about the project.\r\rVisit the GitHub project page for more detailed documentation on the application features or troubleshooting tips.\r\rIf you'd like to re-run this tutorial, you can do so from here.\r\rEnjoy! ;)");
        }
    }
}
