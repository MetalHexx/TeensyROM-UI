using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage;
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
        private int _sdCount = 0;
        private int _usbCount = 0;

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
            result = await _dialog.ShowConfirmation("Terminal View", "On this screen, you will find a few utility features that might come in handy.\r\r•  Ping: This is useful for troubleshooting connectivity. \r•  Reset: Will reset the C64. \r•  Logs: Useful for troubleshooting or debugging. \r•  CLI: Will send serial commands to the TR");

            if (!result)
            {
                await Complete();
                return;
            }
            if (connected)
            {
                result = await _dialog.ShowConfirmation("Connection", "I see you're already connected, so we'll head straight over to the settings view to get things configured.");

                if (!result)
                {
                    await Complete();
                    return;
                }
                await OnSettings();
            }
            else 
            {
                await OnConnectable();
            }
        }

        public async Task OnConnectable()
        {
            var currentSerialState = await _serial.CurrentState.FirstAsync();

            var result = false;

            if (currentSerialState is not SerialConnectableState)
            {
                result = await _dialog.ShowConfirmation("No COM Ports Detected", "Things to try before we proceed: \r\r•  Make sure your USB cable is seated properly \r•  Try a different USB cable \r•  Make sure the C64 is turned on or try restarting it \r\rOnce I detect COM ports, we'll go to the next step.");
            }

            _serial.CurrentState
                .Where(s => s is SerialConnectableState)
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async _ =>
                {
                    var result = await _dialog.ShowConfirmation("Connect to the TR", "Click on the Connect button.  If things go ok, we'll continue to the next step.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }                    
                    OnConnected();
                });
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
                    result = await _dialog.ShowConfirmation("Settings", "Let's head over to the settings view and get your preferences configured.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    await OnSettings();
                });
        }

        private async Task OnSettings()
        {
            _navigation.NavigateTo(NavigationLocation.Settings);

            var result = await _dialog.ShowConfirmation("Settings", "There are various settings here to customize the application behavior to your preferences.  \r\rMouse over the different settings to see a tooltip for what they do.\r\rNote: Help tooltips are present on most controls in the app. ");

            if (!result)
            {
                await Complete();
                return;
            }
            result = await _dialog.ShowConfirmation("MIDI Settings", "You'll see there is also a MIDI tab.  If you have interest in DJing or alternate control methods, there are a lot of options available.");

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

                result = await _dialog.ShowConfirmation("Copy Some Files!", $"Copy some files to your SD or USB storage now before we start the indexing process.  \r\rTotally optional, but strongly recommended, consider copying HVSC and OneLoad64 onto your {_settings.StorageType} storage. You can always do this later if you want.\r\rFor information on the HVSC or OneLoad64, go to the help screen later.\r\rClicking \"Ok\" will start the indexing process.");

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
            var result = await _dialog.ShowConfirmation("Let's navigate back to the terminal screen.  You can watch as all the files on your TeensyROM storage are indexed.");
            
            _navigation.NavigateTo(NavigationLocation.Terminal);

            _dialog.ShowNoClose("Indexing SD Storage", "This may take a few minutes.  Don't touch your commodore while indexing is in progress!");

            _settings.StorageType = TeensyStorageType.SD;
            _settingsService.SaveSettings(_settings);
            await Task.Delay(1000);
            await _storage.CacheAll(StorageConstants.Remote_Path_Root);
            _sdCount = _storage.GetCacheSize();
            _dialog.HideNoClose();

            await OnCacheUSB();
        }

        public async Task OnCacheUSB()
        {
            _dialog.ShowNoClose("Indexing USB Storage", "This may take a few minutes.  Don't touch your commodore while indexing is in progress.");
            _settings.StorageType = TeensyStorageType.USB;
            _settingsService.SaveSettings(_settings);
            await Task.Delay(3000);
            await _storage.CacheAll(StorageConstants.Remote_Path_Root);
            _usbCount = _storage.GetCacheSize();

            _settings.StorageType = _usbCount > _sdCount ? TeensyStorageType.USB : TeensyStorageType.SD;
            _settingsService.SaveSettings(_settings);

            if (_usbCount == 0 && _sdCount == 0) 
            {
                _dialog.HideNoClose();
                await _dialog.ShowConfirmation("File Indexing Completed", $"I noticed nothing was indexed.  \r\r• Do you have TR storage installed?  \r\r• Check your TR storage and make sure it's working ok.\r\r• Restart the tutorial once you have storage sorted out.");
                return;
            }
            _dialog.HideNoClose();

            if (_usbCount == 0 || _sdCount == 0) 
            {
                var possibleErrorResult = await _dialog.ShowConfirmation("File Indexing Completed", $"Note: It's possible you saw an error.  This is expected if you either have SD or USB but not both connected to your TeensyROM device.");

                if (!possibleErrorResult)
                {
                    await Complete();
                    return;
                }
            }

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

            var result = await _dialog.ShowConfirmation("Discovery View", $"In the \"Discover\" view, you can launch music, games, images or text files.\r\rYou can also easily install TeensyROM Firmware (hex file) updates from here.");

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

            result = await _dialog.ShowConfirmation("Re-indexing Storage", $"If you make changes to your selected storage outside of this application, you can re-index by clicking on the down arrow button next to the storage dropdown.");

            if (!result)
            {
                await Complete();
                return;
            }

            result = await _dialog.ShowConfirmation("Feeling Lucky?", "Let's try discovering something to play. \r\rClick on the \"Roll Dice\" button near the bottom of the screen.");

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

                    var result = await _dialog.ShowConfirmation("Random Launch", $"I see you discoverd a {itemType} called {file.Name}, nice! ");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Filters", $"Notice the \"All\" filter is selected in the lower left.  This means any file type will be launched randomly when you roll the dice.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Filters", $"Try selecting the \"Games\" or \"Music\" filter.  This will also randomly launch files of the chosen type.");

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

                    var result = await _dialog.ShowConfirmation("Random Launch", $"Nice, you found a {itemType} called {file.Name}.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Play Toolbar", "The \"Discover\" view functions like a media player.  Clicking \"Previous\" or \"Next\" will behave differently depending on the mode and filter you have selected.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Shuffle Mode", "Currently we're in \"Shuffle Mode\". This is indicated by the blue crossed arrows on the right side of the player.  \r\rVarious functions will launch a random file in this mode.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Shuffle Mode", "For example, clicking \"Next\" to launch the next random file.  The filter will determine the type of file that is launched.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Normal Play Mode", "Try clicking the \"Shuffle Mode\" button to disable it.  Then click the \"Next\" button.");

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
                    result = await _dialog.ShowConfirmation("Normal Play Mode", "Launching a file directly from a directory will also disable shuffle mode.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Continuous Play", $"All file types support continuous play.  \r\r• When a SIDs play time ends, it automatically plays the next SID. \r• For Games and Images, you can set an optional play timer for a similar behavior as SIDs.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Play Timer", $"When playing a game or image, you will notice a \"Stopwatch\" icon in the \"Play\" toolbar.\r\rClicking the \"Stopwatch\" button will enable continuous play on games and images.  You can select a play time length that best suits your needs.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Play Timer", $"There are a lot of interesting uses for this:\r\r• Create a screensaver with images.\r\r• Continuously stream demos\r\r• Demo Booth at a convention");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Advanced Mode", $"Try clicking the gear icon to reveal some advanced features:\r\r• SID Speed Control\r\r• Voice Muting\r\r• Additional timer and launch options.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Seek / Scrub", $"When playing SIDs, you can slide the progress bar to seek to track. Forward OR Reverse!  \r\rTry \"Insane\" mode for some fun. ;)");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("SID DJing", $"All these options combined with a MIDI controller make this a capable SID DJ tool!  Check out the demos in the help section.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Tag Your Favorites!", $"Whenever you see a \"Heart\" button throughout the application, when you click it, the file will be saved to the /favorites folder to TeensyROM storage device.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Playlists", $"You can create playlists with the \"List\" button next to the heart on the play toolbar.  Click this to add items to custom playlists.  \r\rThese will be saved to TeensyROM storage under the /playlists folder.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Search", $"In the upper right, you can search for files.\r\rTip: Make sure you index your storage to get the most out of search and randomization features.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Search Phrases", $"You can search for specific phrases by grouping keywords together in double quotes.\r\rEx: \"Iron Maiden\" Aces High");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }
                    result = await _dialog.ShowConfirmation("Search Required Terms", $"If a search result MUST have a specific keyword or phrase, you can place a + in front of it\r\rEx: +\"Iron Maiden\" Aces +High.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Search Filtering", $"Special Note: \r\rFilters will not trigger a file launch when viewing search results.  Instead, search results will be filtered by the selected filter type.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("", $"I hope enjoy the UI and find some creative ways to use it! :)  \r\rThere are other features you can learn about in the docs or tooltips throughout the app.  \r\rLet's head over to the \"Help\" section now.");

                    await Complete();
                });
        }

        public Task Complete()
        {
            var settings = _settingsService.GetSettings();
            settings.FirstTimeSetup = false;
            _settingsService.SaveSettings(settings);

            _navigation.NavigateTo(NavigationLocation.Help);

            return _dialog.ShowConfirmation("Setup Wizard Complete", "The help section is a great place to find out more about the project.  \r\rBe sure to check out the Demo/Tutorial videos for more help!\r\rVisit the GitHub project page for more detailed documentation on the application features or troubleshooting tips.\r\rIf you'd like to re-run this tutorial, you can do so from here.\r\rEnjoy! ;)");
        }
    }
}
