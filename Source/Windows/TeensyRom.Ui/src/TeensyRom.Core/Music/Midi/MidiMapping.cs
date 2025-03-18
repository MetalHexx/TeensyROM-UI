using System.Text.Json.Serialization;

namespace TeensyRom.Core.Music.Midi
{
    public class MidiMapping()
    {
        public DJEventType DJEventType { get; set; }
        public MidiEventType MidiEventType { get; set; }
        public MidiDevice Device { get; set; } = new();
        public int MidiChannel { get; set; }
        public int NoteOrCC { get; set; }
        public int? FilterValue { get; set; } //Could be velocity or CC value or nothing (null)

        [JsonIgnore]
        public bool IsEnabled => Device is not null && NoteOrCC > 0;

    }
}