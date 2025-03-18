using ReactiveUI.Fody.Helpers;
using TeensyRom.Core.Music.Midi;

namespace TeensyRom.Ui.Features.Settings
{
    public class MidiMappingViewModel
    {
        [Reactive] public DJEventType DJEventType { get; set; }
        [Reactive] public MidiEventType MidiEventType { get; set; }
        [Reactive] public MidiDeviceViewModel Device { get; set; } = null!;
        [Reactive] public int MidiChannel { get; set; }
        [Reactive] public int NoteOrCC { get; set; }
        [Reactive] public int? RequiredValue { get; set; } //Could be velocity or CC value or no requirement (null)

        public MidiMappingViewModel(MidiMapping m)
        {
            DJEventType = m.DJEventType;
            MidiEventType = m.MidiEventType;
            Device = new MidiDeviceViewModel(m.Device);
            MidiChannel = m.MidiChannel;
            NoteOrCC = m.NoteOrCC;
            RequiredValue = m.FilterValue;
        }

        public MidiMappingViewModel(MidiMappingViewModel m)
        {
            DJEventType = m.DJEventType;
            MidiEventType = m.MidiEventType;
            Device = new MidiDeviceViewModel(m.Device);
            MidiChannel = m.MidiChannel;
            NoteOrCC = m.NoteOrCC;
            RequiredValue = m.RequiredValue;
        }

        public MidiMapping ToMidiMapping()
        {
            return new MidiMapping
            {
                DJEventType = DJEventType,
                MidiEventType = MidiEventType,
                Device = Device.ToMidiDevice(),
                MidiChannel = MidiChannel,
                NoteOrCC = NoteOrCC,
                FilterValue = RequiredValue
            };
        }
    }
}