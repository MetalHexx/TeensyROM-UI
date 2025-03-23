using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Midi;

namespace TeensyRom.Ui.Features.Settings
{
    public abstract class MidiMappingViewModel : ReactiveObject
    {
        [Reactive] public DJEventType DJEventType { get; set; }
        [Reactive] public MidiDeviceViewModel Device { get; set; } = null!;
        [Reactive] public int MidiChannel { get; set; }
        [Reactive] public int? FilterValue { get; set; } //Could be velocity or CC value or no requirement (null)
        [Reactive] public string DisplayName { get; set; } = string.Empty;
        [Reactive] public List<MidiDeviceViewModel> AvailableDevices { get; set; } = [];
        [Reactive] public List<int> AvailableChannels { get; set; } = Enumerable.Range(1, 16).ToList();
        [Reactive] public List<int> AvailableMidiValues { get; set; } = Enumerable.Range(0, 128).ToList();
        public ReactiveCommand<MidiMappingViewModel, Unit> LearnCommand { get; set; }
        public ReactiveCommand<MidiMappingViewModel, Unit> ClearCommand { get; set; }

        protected IMidiService _midiService;
        protected IAlertService _alert;

        public MidiMappingViewModel(MidiMapping m, IMidiService midiService, IAlertService alert)
        {
            _alert = alert;
            _midiService = midiService;
            DisplayName = m.DisplayName;
            AvailableDevices = midiService.GetMidiDevices().Select(d => new MidiDeviceViewModel(d)).ToList();
            DJEventType = m.DJEventType;
            Device = new MidiDeviceViewModel(m.Device);
            MidiChannel = m.MidiChannel;
            FilterValue = m.FilterValue;
            SetupCommands();
        }

        public MidiMappingViewModel(MidiMappingViewModel m, IMidiService midiService, IAlertService alert)
        {
            _alert = alert;
            _midiService = midiService;
            AvailableDevices = midiService.GetMidiDevices().Select(d => new MidiDeviceViewModel(d)).ToList();
            DisplayName = m.DisplayName;
            DJEventType = m.DJEventType;
            Device = new MidiDeviceViewModel(m.Device);
            MidiChannel = m.MidiChannel;
            FilterValue = m.FilterValue;
            SetupCommands();
        }

        public void SetupCommands()
        {
            LearnCommand = ReactiveCommand.CreateFromTask<MidiMappingViewModel>(async m =>
            {
                await HandleLearnCommand();
            }, outputScheduler: RxApp.MainThreadScheduler);

            ClearCommand = ReactiveCommand.CreateFromTask<MidiMappingViewModel>(async m =>
            {
                HandleClearCommand();
            }, outputScheduler: RxApp.MainThreadScheduler);

        }

        public abstract MidiMapping ToMidiMapping();
        public abstract Task HandleLearnCommand();
        public abstract void HandleClearCommand();
    }
}