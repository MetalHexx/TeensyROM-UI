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
        [Reactive] public TeensySettings? Settings { get; set; }
        [Reactive] public MidiSettings? MidiSettings { get; set; }
        [Reactive] public ObservableCollection<MidiDevice> AvailableDevices { get; set; } = [];

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
                    Settings = s;
                    RefreshDeviceReferences(s.MidiSettings);
                });

            SaveSettingsCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: _ => HandleSave(),
                outputScheduler: RxApp.MainThreadScheduler);

            RefreshMidiDevicesCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: _ =>
                {
                    Settings = _settingsService.GetSettings() with { };                    
                    _midiService.RefreshMidi(Settings.MidiSettings);                    
                    RefreshDeviceReferences(Settings!.MidiSettings);
                    return Unit.Default;
                },
                outputScheduler: RxApp.MainThreadScheduler);

            _logsSubscription = _logService.Logs
                .Where(log => !string.IsNullOrWhiteSpace(log))
                .Select(log => _logBuilder.AppendLineRolling(log))
                .Select(_ => _logBuilder.ToString())
                .Subscribe(logs =>
                {
                    Logs = logs;
                });

            
        }

        /// <summary>
        /// Ensures the device references are the same as the one from the AvailableDevices dropdown.
        /// </summary>        
        private void RefreshDeviceReferences(MidiSettings m)
        {            
            AvailableDevices.Clear();
            foreach (var d in _midiService.GetMidiDevices())
            {
                AvailableDevices.Add(d);
            }
            m.CurrentSpeed.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.CurrentSpeed.Device?.Name) ?? m.CurrentSpeed.Device;
            m.CurrentSpeedFine.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.CurrentSpeedFine.Device?.Name) ?? m.CurrentSpeedFine.Device;
            m.FastForward.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.FastForward.Device?.Name) ?? m.FastForward.Device;
            m.HomeSpeed.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.HomeSpeed.Device?.Name) ?? m.HomeSpeed.Device;
            m.Next.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.Next.Device?.Name) ?? m.Next.Device;
            m.NudgeBackward.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.NudgeBackward.Device?.Name) ?? m.NudgeBackward.Device;
            m.NudgeForward.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.NudgeForward.Device?.Name) ?? m.NudgeForward.Device;
            m.PlayPause.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.PlayPause.Device?.Name) ?? m.PlayPause.Device;
            m.Previous.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.Previous.Device?.Name) ?? m.Previous.Device;
            m.Seek.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.Seek.Device?.Name) ?? m.Seek.Device;
            m.SetSpeedMinus50.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.SetSpeedMinus50.Device?.Name) ?? m.SetSpeedMinus50.Device;
            m.SetSpeedPlus50.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.SetSpeedPlus50.Device?.Name) ?? m.SetSpeedPlus50.Device;
            m.Stop.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.Stop.Device?.Name) ?? m.Stop.Device;
            m.Voice1.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.Voice1.Device?.Name) ?? m.Voice1.Device;
            m.Voice2.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.Voice2.Device?.Name) ?? m.Voice2.Device;
            m.Voice3.Device = AvailableDevices.FirstOrDefault(d => d.Name == m.Voice3.Device?.Name) ?? m.Voice3.Device;
        }

        private Unit HandleSave()
        {
            var success = _settingsService.SaveSettings(Settings!);
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