using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Linq;
using TeensyRom.Ui.Features.Help;
using TeensyRom.Ui.Features.Terminal;
using TeensyRom.Ui.Features.Settings;
using MaterialDesignThemes.Wpf;
using TeensyRom.Core.Storage;
using TeensyRom.Core.Serial;
using TeensyRom.Ui.Services;
using TeensyRom.Core.Serial.State;
using System.Reflection;
using System;
using TeensyRom.Ui.Features.Discover;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Settings;

namespace TeensyRom.Ui.Features.NavigationHost
{
    public class NavigationHostViewModel : ReactiveObject
    {
        public string Version => GetVersion();
        [ObservableAsProperty] public object? CurrentViewModel { get; }
        [ObservableAsProperty] public object? NavigationItems { get; }        
        [ObservableAsProperty] public bool SerialBusy { get; }
        [Reactive] public string Title { get; set; } = "TeensyROM";
        [Reactive] public bool IsNavOpen { get; set; }
        [Reactive] public bool ControlsEnabled { get; set; } //TODO: Track down why I need this property.  I had to put this here to stop a bunch of errors from throwing in the output window.
        public SnackbarMessageQueue MessageQueue { get; private set; }
        public ReactiveCommand<NavigationItem, Unit>? NavigateCommand { get; private set; }
        public ReactiveCommand<Unit, Unit>? ToggleNavCommand { get; private set; }

        private readonly INavigationService _navService;
        private readonly ISetupService _setup;
        private readonly ISerialStateContext _serialContext;
        private readonly IFileWatchService _watchService;
        private readonly IProgressService _progressService;
        private readonly ISettingsService _settingsService;

        [Reactive] public bool TriggerAnimation { get; set; } = true;

        public NavigationHostViewModel(INavigationService navStore, ISetupService setup, ISerialStateContext serialState, IFileWatchService watchService, ISnackbarService alert, IProgressService progressService, ISettingsService settingsService, HelpViewModel help, TerminalViewModel terminal, SettingsViewModel settings, DiscoverViewModel discover)
        {
            _navService = navStore;
            _setup = setup;
            _serialContext = serialState;
            _watchService = watchService;
            _progressService = progressService;
            _settingsService = settingsService;
            MessageQueue = alert.MessageQueue;
            RegisterModelProperties();
            RegisterModelCommands();
            InitializeNavItems(help, terminal, settings, discover);
            TryNavToDiscover();

            var setupDelay = Task.Delay(1000);

            setupDelay.ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _setup.StartSetup();
                });
            });
        }

        private void TryNavToDiscover()
        {
            var settings = _settingsService.GetSettings();
            if (settings.AutoConnectEnabled && !settings.FirstTimeSetup) 
            { 
                _navService.NavigateTo(NavigationLocation.Discover);
            }
        }

        public void InitializeNavItems(HelpViewModel help, TerminalViewModel terminal, SettingsViewModel settings, DiscoverViewModel discover)
        {   
            _navService.Initialize(NavigationLocation.Terminal, new List<NavigationItem>
            {
                new() {
                    Name = "Terminal",
                    Type = NavigationLocation.Terminal,
                    ViewModel = terminal,
                    Icon = "LanConnect",
                    IsSelected = true,
                    IsEnabled = true
                },
                new() {
                    Name = "Discover",
                    Type = NavigationLocation.Discover,
                    ViewModel = discover,
                    Icon = "CompassRose",
                    IsEnabled = true
                },
                new() {
                    Name = "Settings",
                    Type = NavigationLocation.Settings,
                    ViewModel = settings,
                    Icon = "Gear",
                    IsEnabled = true
                },
                new() {
                    Name = "Help",
                    Type = NavigationLocation.Help,
                    ViewModel = help,
                    Icon = "Help",
                    IsEnabled = true
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

                MessageBus.Current.SendMessage(new NavigatedMessage());

                return Unit.Default;
            }, outputScheduler: ImmediateScheduler.Instance);
        }

        private void RegisterModelProperties()
        {
            _navService.SelectedNavigationView
                .Where(n => n is not null)
                .Select(n => n.ViewModel)
                .ToPropertyEx(this, vm => vm.CurrentViewModel);

            _navService.NavigationItems
                .ToPropertyEx(this, vm => vm.NavigationItems);

            var fileTransferInProgress = _progressService.InProgress
                .WithLatestFrom(_watchService.IsProcessing, (inProgress, watchProcessing) => inProgress || watchProcessing);

            var serialState = _serialContext.CurrentState
                .ObserveOn(RxApp.MainThreadScheduler);

            var serialBusy = serialState
                .Scan((previous: (SerialState?)null, current: (SerialState?)null), (stateTuple, currentState) => (stateTuple.current, currentState))
                .Where(s => s.previous is not null && s.previous is not SerialConnectableState)
                .Select(s => s.current is SerialBusyState);

            serialBusy
                .CombineLatest(fileTransferInProgress, (serialBusy, fileTransfer) => serialBusy || fileTransfer)
                .ToPropertyEx(this, vm => vm.SerialBusy);

            serialState
                .OfType<SerialConnectedState>()                
                .Subscribe(connected =>
                {
                    _navService.Enable(NavigationLocation.Discover);
                });
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
