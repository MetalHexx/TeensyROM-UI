using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Midi;

namespace TeensyRom.Ui.Features.Settings
{
    public class CCMappingViewModel : MidiMappingViewModel
    {
        [Reactive] public int? CCNumber { get; set; }
        [Reactive] public int? RequiredValue { get; set; }
        [Reactive] public CCType CCType { get; set; }

        public List<MidiValueOption> AvailableValueOptions { get; set; }

        public MidiValueOption? SelectedValueOption
        {
            get => AvailableValueOptions.FirstOrDefault(opt => opt.Value == RequiredValue);
            set => RequiredValue = value?.Value;
        }

        public CCMappingViewModel(CCMapping m, IMidiService midiService, IAlertService alert) : base(m, midiService, alert)
        {
            CCNumber = m.CCNumber;
            RequiredValue = m.RequiredValue;
            Device = new MidiDeviceViewModel(m.Device);
            MidiChannel = m.MidiChannel;
            FilterValue = m.FilterValue;
            DisplayName = m.DisplayName;
            CCType = m.CCType;
            Amount = m.Amount;
            AmountEnabled = m.AmountEnabled;
            SetAvailableVelocity();
        }

        public CCMappingViewModel(CCMappingViewModel m, IMidiService midiService, IAlertService alert) : base(m, midiService, alert)
        {
            CCNumber = m.CCNumber;
            RequiredValue = m.RequiredValue;
            DJEventType = m.DJEventType;
            Device = new MidiDeviceViewModel(m.Device);
            MidiChannel = m.MidiChannel;
            FilterValue = m.FilterValue;
            DisplayName = m.DisplayName;
            CCType = m.CCType;
            Amount = m.Amount;
            AmountEnabled = m.AmountEnabled;
            SetAvailableVelocity();
        }

        private void SetAvailableVelocity()
        {
            AvailableValueOptions = [MidiValueOption.NullOption];

            AvailableValueOptions.AddRange(
                Enumerable.Range(0, 128).Select(v => new MidiValueOption { Value = v })
            );
        }

        public override void HandleClearCommand()
        {
            Device = null!;
            Device = new MidiDeviceViewModel(new MidiDevice());
            MidiChannel = 1;
            CCNumber = null;
            RequiredValue = null;         
        }

        public override async Task HandleLearnCommand()
        {
            var message = $"Move a knob or slider on the MIDI device to bind to {DJEventType}";

            _alert.Publish(message);

            var midiResult = await _midiService.GetFirstMidiEvent(MidiEventType.ControlChange);

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
            Device = device;
            MidiChannel = midiResult.Channel;
            CCNumber = midiResult.CCOrNote;            
        }

        public override CCMapping ToMidiMapping()
        {
            return new CCMapping
            {
                DJEventType = DJEventType,
                Device = Device.ToMidiDevice(),
                MidiChannel = MidiChannel,
                FilterValue = FilterValue,
                CCNumber = CCNumber,
                RequiredValue = RequiredValue,                               
                DisplayName = DisplayName,
                CCType = CCType,
                Amount = Amount,
                AmountEnabled = AmountEnabled
            };
        }
    }
}