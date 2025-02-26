using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Midi;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.FeatureTitle;
using TeensyRom.Ui.Helpers.ViewModel;
using TeensyRom.Ui.Services;

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

        [Reactive] public MidiSettingsViewModel MidiSettings { get; set; }
        [Reactive] public ObservableCollection<MidiDeviceViewModel> AvailableDevices { get; set; } = [];

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

        private readonly ISettingsService _settingsService;
        private readonly IAlertService _alert;
        private readonly ILoggingService _logService;
        private readonly IMidiService _midiService;
        private readonly IDisposable _logsSubscription;
        private readonly StringBuilder _logBuilder = new StringBuilder();

        public SettingsViewModel(ISettingsService settings, IAlertService alert, ILoggingService logService, IMidiService midiService)
        {
            FeatureTitle = "Settings";
            _logService = logService;
            _midiService = midiService;
            _settingsService = settings;
            _alert = alert;

            _settingsService.Settings
                .Select(s => (s with { }))
                .Subscribe(s => 
                {
                    MapSettings(s);
                    RefreshDeviceReferences(MidiSettings!);
                });

            SaveSettingsCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: _ => HandleSave(),
                outputScheduler: RxApp.MainThreadScheduler);

            RefreshMidiDevicesCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: _ =>
                {                    
                    var settings = _settingsService.GetSettings() with { };                    
                    _midiService.EngageMidi(settings.MidiSettings);
                    RefreshDeviceReferences(MidiSettings!);
                    return Unit.Default;
                },
                outputScheduler: RxApp.MainThreadScheduler);

            BindMidiCommand = ReactiveCommand.CreateFromTask<MidiMappingViewModel>(async m =>
            {
                if (MidiSettings is null) return;

                var message = m.MidiEventType is MidiEventType.NoteOn or MidiEventType.NoteOff or MidiEventType.NoteChange
                    ? $"Press a note on the MIDI device to bind to {m.DJEventType}"
                    : $"Move a knob or slider on the MIDI device to bind to {m.DJEventType}";

                _alert.Publish(message);

                RefreshDeviceReferences(MidiSettings!);

                var midiResult = await _midiService.GetFirstMidiEvent(m.MidiEventType);

                var device = AvailableDevices.FirstOrDefault(d => d.Id == midiResult.Device.Id);
                if (device == null)
                {
                    _alert.Publish("MIDI device not found in available devices.");
                    return;
                }

                var mapping = MidiSettings.GetAllMappings().FirstOrDefault(x => x.DJEventType == m.DJEventType);
                if (mapping != null)
                {
                    mapping.Device = device;
                    mapping.MidiChannel = midiResult.Channel;
                    mapping.Value = midiResult.Value;
                }
                var midiSettings = MidiSettings;
                MidiSettings = null!;
                MidiSettings = midiSettings;

            }, outputScheduler: RxApp.MainThreadScheduler);

            ClearMidiBindCommand = ReactiveCommand.CreateFromTask<MidiMappingViewModel>(async m =>
            {
                if (MidiSettings is null) return;
                var mapping = MidiSettings.GetAllMappings().FirstOrDefault(x => x.DJEventType == m.DJEventType);
                if (mapping != null)
                {
                    mapping.Device = new MidiDeviceViewModel(new MidiDevice());
                    mapping.MidiChannel = 1;
                    mapping.Value = 1;
                }
                var midiSettings = MidiSettings;
                MidiSettings = null!;
                MidiSettings = midiSettings;

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

        private void MapSettings(TeensySettings s)
        {
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
            MidiSettings = new MidiSettingsViewModel(s.MidiSettings);
        }

        /// <summary>
        /// Ensures the device references are the same as the one from the AvailableDevices dropdown.
        /// </summary>        
        private void RefreshDeviceReferences(MidiSettingsViewModel m)
        {
            var devices = _midiService
                .GetMidiDevices()
                .Select(d => new MidiDeviceViewModel(d))
                .ToList();

            AvailableDevices = new ObservableCollection<MidiDeviceViewModel>(devices);            
            var midiSettings = MidiSettings;
            MidiSettings = null!;
            MidiSettings = new MidiSettingsViewModel(midiSettings, [.. AvailableDevices]);
        }

        private Unit HandleSave()
        {
            var settings = _settingsService.GetSettings() with 
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
                MuteRandomSeek = MuteRandomSeek,
                MidiSettings = MidiSettings.ToMidiSettings()
            };

            var success = _settingsService.SaveSettings(settings);

            if (success)
            {
                _alert.Publish("Settings saved successfully.");
                return Unit.Default;
            }

            _alert.Publish("Error saving settings");
            return Unit.Default;
        }

        public void Dispose()
        {
            _logsSubscription?.Dispose();
        }
    }
}