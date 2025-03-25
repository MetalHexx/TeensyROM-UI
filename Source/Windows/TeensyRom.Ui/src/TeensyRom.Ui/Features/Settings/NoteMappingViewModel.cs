using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Midi;

namespace TeensyRom.Ui.Features.Settings
{
    public class NoteMappingViewModel : MidiMappingViewModel
    {
        [Reactive] public int? NoteNumber { get; set; }
        [Reactive] public NoteEventType NoteEvent { get; set; }
        [Reactive] public int? RequiredVelocity { get; set; }
        public List<MidiValueOption> AvailableVelocityOptions { get; set; }

        public MidiValueOption? SelectedVelocityOption
        {
            get => AvailableVelocityOptions.FirstOrDefault(opt => opt.Value == RequiredVelocity);
            set => RequiredVelocity = value?.Value;
        }

        public NoteMappingViewModel(NoteMapping m, IMidiService midiService, IAlertService alert) : base(m, midiService, alert)
        {
            DisplayName = m.DisplayName;
            NoteNumber = m.NoteNumber;
            NoteEvent = m.NoteEvent;
            RequiredVelocity = m.RequiredVelocity;
            Amount = m.Amount;
            AmountEnabled = m.AmountEnabled;
            SetAvailableVelocity();
        }

        public NoteMappingViewModel(NoteMappingViewModel m, IMidiService midiService, IAlertService alert) : base(m, midiService, alert)
        {
            DisplayName = m.DisplayName;
            NoteNumber = m.NoteNumber;
            NoteEvent = m.NoteEvent;
            RequiredVelocity = m.RequiredVelocity;
            Amount = m.Amount;
            AmountEnabled = m.AmountEnabled;

            this.WhenAnyValue(x => x.RequiredVelocity)
                .Subscribe(v => 
                {
                    Debug.WriteLine($"Selected value: {(v?.ToString() ?? "None")}");
                });

            SetAvailableVelocity();
        }

        private void SetAvailableVelocity()
        {
            AvailableVelocityOptions = [MidiValueOption.NullOption];

            AvailableVelocityOptions.AddRange(
                Enumerable.Range(0, 128).Select(v => new MidiValueOption { Value = v })
            );
        }

        public override void HandleClearCommand()
        {
            Device = null!;
            Device = new MidiDeviceViewModel(new MidiDevice());
            MidiChannel = 1;
            NoteNumber = null;
            RequiredVelocity = null;
            AmountEnabled = AmountEnabled;
        }

        public override async Task HandleLearnCommand()
        {
            var message = $"Press a note on the MIDI device to bind to {DJEventType}";

            _alert.Publish(message);

            var midiResult = await _midiService.GetFirstMidiEvent(MidiEventType.NoteChange);

            if (midiResult is null)
            {
                _alert.Publish("No midi event detected.  Cancelling midi learn.");
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
            NoteNumber = midiResult.CCOrNote;
            RequiredVelocity = null;
        }

        public override NoteMapping ToMidiMapping()
        {
            return new NoteMapping
            {
                DisplayName = DisplayName,
                DJEventType = DJEventType,
                Device = Device.ToMidiDevice(),
                MidiChannel = MidiChannel,
                FilterValue = FilterValue,
                NoteNumber = NoteNumber,
                NoteEvent = NoteEvent,
                RequiredVelocity = RequiredVelocity,
                Amount = Amount,
                AmountEnabled = AmountEnabled
            };
        }
    }
}