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

        public ReactiveCommand<NavigationItem, Unit> NavigateCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> ToggleNavCommand { get; private set; }

        private readonly INavigationService _navService;

        public NavigationHostViewModel(INavigationService navStore, FileTransferViewModel fileTransfer, MidiViewModel midi, HelpViewModel help)
        {
            _navService = navStore;
            RegisterModelProperties();
            RegisterModelCommands();
            InitializeNavItems(fileTransfer, midi, help);
        }     

        public void InitializeNavItems(FileTransferViewModel fileTransfer, MidiViewModel midi, HelpViewModel help)
        {
            _navService.Initialize(NavigationLocation.FileTransfer, new List<NavigationItem>
            {
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
