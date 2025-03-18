using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Midi;
using TeensyRom.Core.Settings;
using TeensyRom.Ui.Controls.FeatureTitle;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Helpers.ViewModel;

namespace TeensyRom.Ui.Features.Settings
{
    public class SettingsViewModel: FeatureViewModelBase, IDisposable
    {
        [Reactive] public FeatureTitleViewModel Title { get; set; } = new("Settings");
        [Reactive] public string Logs { get; set; } = string.Empty;
        [ObservableAsProperty] public bool IsDirty { get; }
        public string WatchDirectoryLocation { get; set; } = string.Empty;
        public string AutoTransferPath { get; set; } = "auto-transfer";
        public bool AutoFileCopyEnabled { get; set; } = false;
        public bool AutoLaunchOnCopyEnabled { get; set; } = true;
        public bool AutoConnectEnabled { get; set; } = true;
        public TeensyFilterType StartupFilter { get; set; } = TeensyFilterType.All;
        public bool StartupLaunchEnabled { get; set; } = true;
        public bool PlayTimerEnabled { get; set; } = false;
        public bool NavToDirOnLaunch { get; set; } = true;
        public bool MuteFastForward { get; set; } = true;
        public bool MuteRandomSeek { get; set; } = true;
        [Reactive] public KnownCartViewModel LastCart { get; set; }
        [Reactive] public ObservableCollection<MidiDeviceViewModel> AvailableDevices { get; set; } = [];
        [Reactive] public bool MidiConfigEnabled { get; set; } = false;
        [Reactive] public bool SaveInProgress { get; private set; }

        [Reactive]
        public List<TeensyFilterType> FilterOptions { get; set; } = Enum
            .GetValues(typeof(TeensyFilterType))
            .Cast<TeensyFilterType>()
            .Where(type => type != TeensyFilterType.Hex)
            .ToList();
        public IEnumerable<int> MidiChannels { get; } = Enumerable.Range(1, 16);
        public IEnumerable<int> PossibleNoteOrCCValues { get; } = Enumerable.Range(0, 127);
        public Interaction<string, bool> ConfirmSave { get; } = new Interaction<string, bool>();

        public ReactiveCommand<Unit, Unit> SaveSettingsCommand { get; set; }
        public ReactiveCommand<Unit, Unit> RefreshMidiDevicesCommand { get; set; }
        public ReactiveCommand<MidiMappingViewModel, Unit> BindMidiCommand { get; set; }
        public ReactiveCommand<MidiMappingViewModel, Unit> ClearMidiBindCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ToggleMidiEnabledCommand { get; }        

        private readonly ISettingsService _settingsService;
        private readonly IAlertService _alert;
        private readonly ILoggingService _logService;
        private readonly IMidiService _midiService;
        private readonly IDisposable _logsSubscription;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public SettingsViewModel(ISettingsService settings, IAlertService alert, ILoggingService logService, IMidiService midiService, INavigationService nav)
        {
            FeatureTitle = "Settings";
            _logService = logService;
            _midiService = midiService;
            _settingsService = settings;
            _alert = alert;

            GetAvailableMidiDevices();

            nav.SelectedNavigationView
                .Where(nav => nav is not null && nav.Type is NavigationLocation.Settings)
                .Subscribe(_ =>
                {
                    ReloadSettings();
                });

            SaveSettingsCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: _ =>
                {
                    Task.Run(() => HandleSave());
                    return Unit.Default;
                },
                outputScheduler: RxApp.MainThreadScheduler);

            RefreshMidiDevicesCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: _ =>
                {
                    Task.Run(() =>
                    {
                        var settings = _settingsService.GetSettings() with { };
                        if (settings.LastCart is null)
                        {
                            _alert.Publish("Please connect to a TR Device before configuring MIDI.");
                            return;
                        }
                        RefreshDeviceReferences(settings.LastCart);
                    });
                    return Unit.Default;
                },
                outputScheduler: RxApp.MainThreadScheduler);

            BindMidiCommand = ReactiveCommand.CreateFromTask<MidiMappingViewModel>(async m =>
            {
                if (LastCart is null) return;

                var message = m.MidiEventType is MidiEventType.NoteOn or MidiEventType.NoteOff or MidiEventType.NoteChange
                    ? $"Press a note on the MIDI device to bind to {m.DJEventType}"
                    : $"Move a knob or slider on the MIDI device to bind to {m.DJEventType}";

                _alert.Publish(message);

                //RefreshDeviceReferences(LastCart.ToKnownCart());

                var midiResult = await _midiService.GetFirstMidiEvent(m.MidiEventType);

                if (midiResult is null)
                {
                    _alert.Publish("Not connected to a TeensyROM cart.");
                    return;
                }

                var device = AvailableDevices.FirstOrDefault(d => d.Id == midiResult.Device.Id);
                if (device == null)
                {
                    _alert.Publish("MIDI device not found in available devices.");
                    return;
                }

                var mapping = LastCart.MidiSettings.GetAllMappings().FirstOrDefault(x => x.DJEventType == m.DJEventType);
                if (mapping != null)
                {
                    mapping.Device = device;
                    mapping.MidiChannel = midiResult.Channel;
                    mapping.NoteOrCC = midiResult.Value;
                }
                var lastCart = LastCart;
                LastCart = null!;
                LastCart = lastCart;

            }, outputScheduler: RxApp.MainThreadScheduler);

            ClearMidiBindCommand = ReactiveCommand.CreateFromTask<MidiMappingViewModel>(async m =>
            {
                if (LastCart is null) return;
                var mapping = LastCart.MidiSettings.GetAllMappings().FirstOrDefault(x => x.DJEventType == m.DJEventType);
                if (mapping != null)
                {
                    mapping.Device = new MidiDeviceViewModel(new MidiDevice());
                    mapping.MidiChannel = 0;
                    mapping.NoteOrCC = -1;
                }
                var lastCart = LastCart;
                LastCart = null!;
                LastCart = lastCart;

            }, outputScheduler: RxApp.MainThreadScheduler);

            _logsSubscription = _logService.Logs
                    .Where(log => !string.IsNullOrWhiteSpace(log))
                    .Select(log => _logBuilder.AppendLineRolling(log))
                    .Select(_ => _logBuilder.ToString())
                    .Subscribe(logs =>
                    {
                        Logs = logs;
                    });


        }

        private void GetAvailableMidiDevices()
        {
            _midiService.DisengageMidi();

            var devices = _midiService
                            .GetMidiDevices()
                            .Select(d => new MidiDeviceViewModel(d))
                            .ToList();

            AvailableDevices = new ObservableCollection<MidiDeviceViewModel>(devices);
        }

        private void ReloadSettings()
        {
            var s = _settingsService.GetSettings();

            WatchDirectoryLocation = s.WatchDirectoryLocation;
            AutoTransferPath = s.AutoTransferPath;
            AutoFileCopyEnabled = s.AutoFileCopyEnabled;
            AutoLaunchOnCopyEnabled = s.AutoLaunchOnCopyEnabled;
            AutoConnectEnabled = s.AutoConnectEnabled;
            StartupFilter = s.StartupFilter;
            StartupLaunchEnabled = s.StartupLaunchEnabled;
            PlayTimerEnabled = s.PlayTimerEnabled;
            NavToDirOnLaunch = s.NavToDirOnLaunch;
            MuteFastForward = s.MuteFastForward;
            MuteRandomSeek = s.MuteRandomSeek;

            if (s.LastCart is not null)
            {
                LastCart = new KnownCartViewModel(s.LastCart, [.. AvailableDevices]);
                MidiConfigEnabled = true;
            }
            else 
            {
                MidiConfigEnabled = false;
            }
        }

        /// <summary>
        /// Ensures the device references are the same as the one from the AvailableDevices dropdown.
        /// </summary>        
        private void RefreshDeviceReferences(KnownCart k)
        {
            GetAvailableMidiDevices();
            var lastCart = LastCart;
            LastCart = null!;
            LastCart = new KnownCartViewModel(lastCart, [.. AvailableDevices]);
        }

        public Unit HandleSave()
        {
            RxApp.MainThreadScheduler.Schedule(() => SaveInProgress = true);

            var settings = _settingsService.GetSettings();

            settings = settings with 
            {
                WatchDirectoryLocation = WatchDirectoryLocation,
                AutoTransferPath = AutoTransferPath,
                AutoFileCopyEnabled = AutoFileCopyEnabled,
                AutoLaunchOnCopyEnabled = AutoLaunchOnCopyEnabled,
                AutoConnectEnabled = AutoConnectEnabled,
                StartupFilter = StartupFilter,
                StartupLaunchEnabled = StartupLaunchEnabled,
                PlayTimerEnabled = PlayTimerEnabled,
                NavToDirOnLaunch = NavToDirOnLaunch,
                MuteFastForward = MuteFastForward,
                MuteRandomSeek = MuteRandomSeek
            };


            if (LastCart is not null)
            {
                var cart = settings.KnownCarts.First(c => c.DeviceHash == LastCart.DeviceHash);

                var updatedCart = cart with
                {
                    MidiSettings = LastCart.MidiSettings.ToMidiSettings(),
                    Name = LastCart.Name,
                    DeviceHash = LastCart.DeviceHash,
                    PnpDeviceId = LastCart.PnpDeviceId,
                    ComPort = LastCart.ComPort
                };
                settings.KnownCarts.Remove(cart);
                settings.KnownCarts.Add(updatedCart);
                settings = settings with { LastCart = updatedCart };
            }

            var success = _settingsService.SaveSettings(settings);

            if (success)
            {
                _alert.Publish("Settings saved successfully.");
                ReloadSettings();
                RxApp.MainThreadScheduler.Schedule(() => SaveInProgress = false);
                return Unit.Default;
            }

            _alert.Publish("Error saving settings");
            RxApp.MainThreadScheduler.Schedule(() => SaveInProgress = false);
            return Unit.Default;
        }

        public void Dispose()
        {
            _logsSubscription?.Dispose();
        }
    }
}