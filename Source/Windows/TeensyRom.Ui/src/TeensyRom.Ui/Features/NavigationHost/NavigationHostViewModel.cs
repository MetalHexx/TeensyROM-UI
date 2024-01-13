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

namespace TeensyRom.Ui.Features.NavigationHost
{
    public class NavigationHostViewModel : ReactiveObject
    {
        [ObservableAsProperty] public object CurrentViewModel { get; }
        [ObservableAsProperty] public object NavigationItems { get; }
        [ObservableAsProperty] public bool IsNavOpen { get; }
        [ObservableAsProperty] public bool SerialBusy { get; }
        [Reactive] public string Title { get; set; } = "TeensyROM";
        //TODO: Track down why I need this property.  I had to put this here to stop a bunch of errors from throwing in the output window.
        [Reactive] public bool ControlsEnabled { get; set; }
        public SnackbarMessageQueue MessageQueue { get; private set; }
        public ReactiveCommand<NavigationItem, Unit> NavigateCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> ToggleNavCommand { get; private set; }

        private readonly INavigationService _navService;
        private readonly ISerialPortState _serialState;

        [Reactive] public bool TriggerAnimation { get; set; } = true;

        public NavigationHostViewModel(INavigationService navStore, ISnackbarService alert, FilesViewModel files, MusicViewModel music, HelpViewModel help, ConnectViewModel connect, SettingsViewModel settings, ISerialPortState serialState)
        {
            _navService = navStore;
            _serialState = serialState;
            MessageQueue = alert.MessageQueue;
            RegisterModelProperties();
            RegisterModelCommands();
            InitializeNavItems(files, music, help, connect, settings);
        }     

        public void InitializeNavItems(FilesViewModel files, MusicViewModel midi, HelpViewModel help, ConnectViewModel connect, SettingsViewModel settings)
        {
            _navService.Initialize(NavigationLocation.Connect, new List<NavigationItem>
            {
                new NavigationItem
                {
                    Name = "Connect",
                    Type = NavigationLocation.Connect,
                    ViewModel = connect,
                    Icon = "LanConnect"
                },
                new NavigationItem
                {
                    Name = "File Explorer",
                    Type = NavigationLocation.Files,
                    ViewModel = files,
                    Icon = "FileArrowLeftRightOutline"
                },
                new NavigationItem
                {
                    Name = "Midi Tools",
                    Type = NavigationLocation.Music,
                    ViewModel = midi,
                    Icon = "MusicClefTreble"
                },
                new NavigationItem
                {
                    Name = "Settings",
                    Type = NavigationLocation.Settings,
                    ViewModel = settings,
                    Icon = "Gear"
                },
                new NavigationItem
                {
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
                
                return Unit.Default;
            }, outputScheduler: ImmediateScheduler.Instance);

            ToggleNavCommand = ReactiveCommand.Create<Unit, Unit>(n =>
                _navService.ToggleNav(), outputScheduler: ImmediateScheduler.Instance);

        }

        private void RegisterModelProperties()
        {
            _navService.SelectedNavigationView
                            .Where(n => n is not null)
                            .Select(n => n.ViewModel)
                            .ToPropertyEx(this, vm => vm.CurrentViewModel);

            _navService.NavigationItems
                .ToPropertyEx(this, vm => vm.NavigationItems);

            _navService.IsNavOpen.ToPropertyEx(this, vm => vm.IsNavOpen);
            _serialState.IsBusy.ToPropertyEx(this, vm => vm.SerialBusy);
        }
    }
}
