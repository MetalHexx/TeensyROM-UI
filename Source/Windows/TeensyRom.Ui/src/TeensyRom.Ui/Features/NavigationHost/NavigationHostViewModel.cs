using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TeensyRom.Ui.Features.FileTransfer;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Linq;
using TeensyRom.Ui.Features.Help;
using TeensyRom.Ui.Features.Midi;
using TeensyRom.Ui.Features.Connect;
using TeensyRom.Ui.Features.Settings;
using MaterialDesignThemes.Wpf;
using System.Windows.Threading;
using System;

namespace TeensyRom.Ui.Features.NavigationHost
{
    public class NavigationHostViewModel : ReactiveObject
    {
        [ObservableAsProperty]
        public object CurrentViewModel { get; }

        [ObservableAsProperty]
        public object NavigationItems { get; }

        [ObservableAsProperty]
        public bool IsNavOpen { get; }

        public SnackbarMessageQueue MessageQueue { get; private set; }

        public ReactiveCommand<NavigationItem, Unit> NavigateCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> ToggleNavCommand { get; private set; }

        private readonly INavigationService _navService;
        private readonly ISnackbarService _snackbar;

        public NavigationHostViewModel(INavigationService navStore, ISnackbarService snackbar, FileTransferViewModel fileTransfer, MidiViewModel midi, HelpViewModel help, ConnectViewModel connect, SettingsViewModel settings)
        {
            _navService = navStore;            
            MessageQueue = snackbar.MessageQueue;
            RegisterModelProperties();
            RegisterModelCommands();
            InitializeNavItems(fileTransfer, midi, help, connect, settings);
        }     

        public void InitializeNavItems(FileTransferViewModel fileTransfer, MidiViewModel midi, HelpViewModel help, ConnectViewModel connect, SettingsViewModel settings)
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
                    Name = "File Transfer",
                    Type = NavigationLocation.FileTransfer,
                    ViewModel = fileTransfer,
                    Icon = "FileArrowLeftRightOutline"
                },
                new NavigationItem
                {
                    Name = "Midi Tools",
                    Type = NavigationLocation.Midi,
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
                _navService.NavigateTo(n.Id);
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
        }
    }
}
