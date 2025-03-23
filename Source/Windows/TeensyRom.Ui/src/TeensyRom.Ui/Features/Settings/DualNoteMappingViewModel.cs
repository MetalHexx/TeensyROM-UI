using ReactiveUI.Fody.Helpers;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Midi;

namespace TeensyRom.Ui.Features.Settings
{
    public class DualNoteMappingViewModel : NoteMappingViewModel
    {
        [Reactive] public NoteEventType NoteEvent2 { get; set; }

        public DualNoteMappingViewModel(DualNoteMapping m, IMidiService midiService, IAlertService alert) : base(m, midiService, alert)
        {
            NoteEvent2 = m.NoteEvent2;
            DJEventType = m.DJEventType;
            Device = new MidiDeviceViewModel(m.Device);
            MidiChannel = m.MidiChannel;
            FilterValue = m.FilterValue;
            DisplayName = m.DisplayName;
            Amount = m.Amount;
            AmountEnabled = m.AmountEnabled;
        }

        public DualNoteMappingViewModel(DualNoteMappingViewModel m, IMidiService midiService, IAlertService alert) : base(m, midiService, alert)
        {
            NoteEvent2 = m.NoteEvent2;
            DJEventType = m.DJEventType;
            Device = new MidiDeviceViewModel(m.Device);
            MidiChannel = m.MidiChannel;
            FilterValue = m.FilterValue;
            DisplayName = m.DisplayName;
            Amount = m.Amount;
            AmountEnabled = m.AmountEnabled;
        }

        public override DualNoteMapping ToMidiMapping()
        {
            return new DualNoteMapping
            {
                DJEventType = DJEventType,
                Device = Device.ToMidiDevice(),
                MidiChannel = MidiChannel,
                FilterValue = FilterValue,
                NoteNumber = NoteNumber,
                NoteEvent = NoteEvent,
                RequiredVelocity = RequiredVelocity,
                NoteEvent2 = NoteEvent2,
                DisplayName = DisplayName,
                Amount = Amount,
            };
        }
    }
}