using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Linq;
using TeensyRom.Ui.Features.Help;
using TeensyRom.Ui.Features.Connect;
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
        public ReactiveCommand<Unit, Unit>? ToggleNavCommand { get; private set; }

        private readonly INavigationService _navService;
        private readonly ISetupService _setup;
        private readonly ISerialStateContext _serialContext;

        [Reactive] public bool TriggerAnimation { get; set; } = true;

        public NavigationHostViewModel(INavigationService navStore, ISetupService setup, ISerialStateContext serialState, ISnackbarService alert, HelpViewModel help, ConnectViewModel connect, SettingsViewModel settings, DiscoverViewModel discover)
        {
            _navService = navStore;
            _setup = setup;
            _serialContext = serialState;
            MessageQueue = alert.MessageQueue;
            RegisterModelProperties();
            RegisterModelCommands();
            InitializeNavItems(help, connect, settings, discover);

            var setupDelay = Task.Delay(1000);

            setupDelay.ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _setup.StartSetup();
                });
            });
        }

        public void InitializeNavItems(HelpViewModel help, ConnectViewModel connect, SettingsViewModel settings, DiscoverViewModel discover)
        {
            _navService.Initialize(NavigationLocation.Connect, new List<NavigationItem>
            {
                new() {
                    Name = "Connect",
                    Type = NavigationLocation.Connect,
                    ViewModel = connect,
                    Icon = "LanConnect",
                    IsSelected = true
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

            _serialContext.CurrentState           
                .Scan((previous: (SerialState?)null, current: (SerialState?)null), (stateTuple, currentState) => (stateTuple.current, currentState))    
                .Where(s => s.previous is not null && s.previous is not SerialConnectableState)
                .Select(s => s.current is SerialBusyState)
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
