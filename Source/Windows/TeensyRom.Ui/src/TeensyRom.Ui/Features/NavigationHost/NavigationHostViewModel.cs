using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Linq;
using TeensyRom.Ui.Features.Help;
using TeensyRom.Ui.Features.Music;
using TeensyRom.Ui.Features.Connect;
using TeensyRom.Ui.Features.Settings;
using MaterialDesignThemes.Wpf;
using TeensyRom.Core.Storage;
using TeensyRom.Ui.Features.Files;
using TeensyRom.Core.Serial;
using TeensyRom.Ui.Services;
using TeensyRom.Core.Serial.State;
using TeensyRom.Ui.Features.Games;
using System.Reflection;
using System;
using TeensyRom.Ui.Features.Discover;

namespace TeensyRom.Ui.Features.NavigationHost
{
    public class NavigationHostViewModel : ReactiveObject
    {
        public string Version => GetVersion();
        [ObservableAsProperty] public object? CurrentViewModel { get; }
        [ObservableAsProperty] public object? NavigationItems { get; }        
        [ObservableAsProperty] public bool SerialBusy { get; set; }
        [Reactive] public string Title { get; set; } = "TeensyROM";
        [Reactive] public bool IsNavOpen { get; set; }
        [Reactive] public bool ControlsEnabled { get; set; } //TODO: Track down why I need this property.  I had to put this here to stop a bunch of errors from throwing in the output window.
        public SnackbarMessageQueue MessageQueue { get; private set; }
        public ReactiveCommand<NavigationItem, Unit>? NavigateCommand { get; private set; }
        public ReactiveCommand<Unit, Unit>? OpenNavCommand { get; private set; }
        public ReactiveCommand<Unit, Unit>? CloseNavCommand { get; private set; }

        private readonly INavigationService _navService;
        private readonly ISerialStateContext _serialContext;

        [Reactive] public bool TriggerAnimation { get; set; } = true;

        public NavigationHostViewModel(INavigationService navStore, ISerialStateContext serialState, ISnackbarService alert, FilesViewModel files, MusicViewModel music, GamesViewModel games, HelpViewModel help, ConnectViewModel connect, SettingsViewModel settings, DiscoverViewModel discover)
        {
            _navService = navStore;
            _serialContext = serialState;
            MessageQueue = alert.MessageQueue;
            RegisterModelProperties();
            RegisterModelCommands();
            InitializeNavItems(files, music, games, help, connect, settings, discover);
        }

        public void InitializeNavItems(FilesViewModel files, MusicViewModel midi, GamesViewModel games, HelpViewModel help, ConnectViewModel connect, SettingsViewModel settings, DiscoverViewModel discover)
        {
            _navService.Initialize(NavigationLocation.Connect, new List<NavigationItem>
            {
                new() {
                    Name = "Connect",
                    Type = NavigationLocation.Connect,
                    ViewModel = connect,
                    Icon = "LanConnect"
                },
                new() {
                    Name = "File Explorer",
                    Type = NavigationLocation.Files,
                    ViewModel = files,
                    Icon = "FileArrowLeftRightOutline"
                },
                new() {
                    Name = "Music Player",
                    Type = NavigationLocation.Music,
                    ViewModel = midi,
                    Icon = "MusicClefTreble"
                },
                new() {
                    Name = "Games",
                    Type = NavigationLocation.Games,
                    ViewModel = games,
                    Icon = "Ghost"
                },
                new() {
                    Name = "Discover",
                    Type = NavigationLocation.Discover,
                    ViewModel = discover,
                    Icon = "CompassRose"
                },
                new() {
                    Name = "Settings",
                    Type = NavigationLocation.Settings,
                    ViewModel = settings,
                    Icon = "Gear"
                },
                new() {
                    Name = "Help",
                    Type = NavigationLocation.Help,
                    ViewModel = help,
                    Icon = "Help"
                },
            });
        }

        private void RegisterModelCommands()
        {
            NavigateCommand = ReactiveCommand.Create<NavigationItem, Unit>(n =>
            {
                TriggerAnimation = true;

                _navService.NavigateTo(n.Id);                
                
                TriggerAnimation = false;

                if (IsNavOpen)
                {
                    IsNavOpen = false;
                    MessageBus.Current.SendMessage(new NavAnimationMessage { NavMenuState = NavMenuState.Closed });
                }

                return Unit.Default;
            }, outputScheduler: ImmediateScheduler.Instance);

            OpenNavCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: n => 
                {
                    IsNavOpen = true;
                    MessageBus.Current.SendMessage(new NavAnimationMessage { NavMenuState = NavMenuState.Opened });
                    return Unit.Default;
                },
                outputScheduler: ImmediateScheduler.Instance);

            CloseNavCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: n =>
                {
                    IsNavOpen = false;
                    MessageBus.Current.SendMessage(new NavAnimationMessage { NavMenuState = NavMenuState.Closed });
                    return Unit.Default;
                },
                outputScheduler: ImmediateScheduler.Instance);

        }

        private void RegisterModelProperties()
        {
            _navService.SelectedNavigationView
                            .Where(n => n is not null)
                            .Select(n => n.ViewModel)
                            .ToPropertyEx(this, vm => vm.CurrentViewModel);

            _navService.NavigationItems
                .ToPropertyEx(this, vm => vm.NavigationItems);

            _serialContext.CurrentState
                .Select(s => s is SerialBusyState)
                .Throttle(TimeSpan.FromMilliseconds(1000))
                .ToPropertyEx(this, vm => vm.SerialBusy);
        }

        public static string GetVersion()
        {
            string version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown version";
            var plusIndex = version.IndexOf('+');
            version = plusIndex > -1 ? version.Substring(0, plusIndex) : version;
            return $"v{version}";
        }
    }
}
