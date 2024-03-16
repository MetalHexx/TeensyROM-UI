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
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.NavigationHost
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

        public SetupService(ISettingsService settingsService, INavigationService navigation, ISerialStateContext serial, IDialogService dialog, ICachedStorageService storage, IDiscoverContext discover)
        {
            _settingsService = settingsService;
            _navigation = navigation;
            _serial = serial;
            _dialog = dialog;
            _storage = storage;
            _discover = discover;
        }
        public async Task StartSetup()
        {   
            var settings = _settingsService.GetSettings();

            if (settings.FirstTimeSetup) return;

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
                    var result = await _dialog.ShowConfirmation("Successful Connection!", "Let's head over to the settings view and get your libraries configured.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    _navigation.NavigateTo(NavigationLocation.Settings);

                    result = await _dialog.ShowConfirmation("Add Some Files", "Totally optional, but strongly recommended, consider copying HVSC and OneLoad64 onto your SD or USB storage. Make note of the directory paths you put them in.\r\rYou can always do this later if you want.");

                    result = await _dialog.ShowConfirmation("Select your storage device and set up your library locations.  \r\r We'll continue once you've saved your settings.");

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

                await OnCache();                
            });
        }

        public async Task OnCache()
        {
            //var cachingTask = _storage.CacheAll();
            var result = await _dialog.ShowConfirmation("Let's navigate back to the connection screen.  You can watch as all the files are read from the TR.\r\rHang tight as this will likely take several minutes if you have thousands of files.");
            _navigation.NavigateTo(NavigationLocation.Connect);

            _serial.CurrentState
                .Where(s => s is SerialConnectedState)
                .Take(1)
                .ObserveOn(RxApp.MainThreadScheduler).Subscribe(async _ =>
                {
                    var result = await _dialog.ShowConfirmation("Caching Completed", "Now that your file information has been cached, lets head over to the Discover view and do some exploring.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    _navigation.NavigateTo(NavigationLocation.Discover);

                    result = await _dialog.ShowConfirmation("Discover View", "If a file explorer and media player had a baby, it would be the \"Discover\" view.  Let's test our luck and see what we can find.  \r\rClick the dice button in the upper right toolbar.");

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

                    var result = await _dialog.ShowConfirmation("Random Launch", $"I see you discoverd a {itemType} called \"{file.Name}\", nice! \r\rNotice \"All\" is selected in the lower left.  Currently, either Games or Music will be launched randomly.  \r\rTry selecting the \"Games\" library and click \"Next\" button on the play toolbar this time.  A new random game should launch.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    OnLaunchGame();
                });
            //await _dialog.ShowConfirmation("You're all set!", "You can now start using TeensyROM to explore your infinitely massive collection.  \r\rI'll take you to the help section now where you can find out more information about the application. \r\r Enjoy! ;)");

            //_navigation.NavigateTo(NavigationLocation.Help);
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

                    if(file is GameItem)
                    {
                        result = await _dialog.ShowConfirmation("Random Game Launched", $"Nice, you found a game called {file.Name}.  \r\rIf you were to press the previous button, it would go back to the previous game.  Yes, your random play history is tracked.");
                    }

                    if (file is SongItem)
                    {
                        result = await _dialog.ShowConfirmation("Random Music Launched", $"It looks like you found a song.  Remember to click \"Games\" or \"Music\" library to filter your results.");
                    }

                    result = await _dialog.ShowConfirmation("Note", $"If you play music and the song ends, as you would expect, the next track will play automatically.  You can try that later.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("Shuffle Mode", "Similar to a music player, shuffle mode will cause the \"Next\" button to play a random file from anywhere in your collection.  \r\rSince you clicked the dice before, you're automatically in Shuffle mode.  This is indicated by the blue \"cross arrow\" button on the right side of the play toolbar. \r\rTry clicking it to disable shuffle and click \"Next\" again.");

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

                    var result = await _dialog.ShowConfirmation("Normal Play Mode", $"The next file in the current directory should have launched if you turned off Shuffle mode.");

                    if (!result)
                    {
                        await Complete();
                        return;
                    }

                    result = await _dialog.ShowConfirmation("", $"Hopefully, this brief introduction gave you a basic idea of how to randomly discover new content.  \r\rThere are more features like search, favorite tagging and file transfer that you should try out. \r\rFor now, let's head to the help section.");

                    await Complete();
                });
        }

        public Task Complete()
        {
            var settings = _settingsService.GetSettings();
            settings.FirstTimeSetup = false;
            _settingsService.SaveSettings(settings);

            _navigation.NavigateTo(NavigationLocation.Help);

            return _dialog.ShowConfirmation("Setup Wizard Complete", "The help section is a great place to find out more about the application.  \r\rThere are more features to explain, so go there for more information.  You can also find links to the guides and the source code.  \r\rEnjoy! ;)");
        }
    }
}
