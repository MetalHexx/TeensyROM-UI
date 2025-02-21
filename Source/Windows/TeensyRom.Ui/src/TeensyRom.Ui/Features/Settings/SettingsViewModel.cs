using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [ObservableAsProperty] public TeensySettings? Settings { get; }        
        [Reactive] public IEnumerable<MidiDevice> AvailableDevices { get; set; } = [];

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

            AvailableDevices = _midiService.GetMidiDevices();            

            _settingsService.Settings
                .Select(s => s with { })
                .Select(s =>
                {
                    //Ensures the same reference from the dropdown is set.  Also makes the selected device blank to indicate to the user it's missing.
                    //TODO: Clean up smelly code.
                    s.MidiSettings.CurrentSpeed.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.CurrentSpeed.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.FastForward.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.FastForward.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.HomeSpeed.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.HomeSpeed.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.Next.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.Next.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.NudgeBack.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.NudgeBack.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.NudgeFoward.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.NudgeFoward.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.PlayPause.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.PlayPause.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.Previous.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.Previous.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.Seek.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.Seek.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.SetSpeedMinus50.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.SetSpeedMinus50.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.SetSpeedPlus50.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.SetSpeedPlus50.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.Stop.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.Stop.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.Voice1.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.Voice1.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.Voice2.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.Voice2.Device?.Name) ?? new MidiDevice();
                    s.MidiSettings.Voice3.Device = AvailableDevices.FirstOrDefault(d => d.Name == s.MidiSettings.Voice3.Device?.Name) ?? new MidiDevice();

                    return s;
                })
                .ToPropertyEx(this, vm => vm.Settings);

            SaveSettingsCommand = ReactiveCommand.Create<Unit, Unit>(
                execute: n => HandleSave(),
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