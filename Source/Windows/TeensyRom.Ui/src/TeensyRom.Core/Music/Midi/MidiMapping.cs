using System.Text.Json.Serialization;

namespace TeensyRom.Core.Music.Midi
{
    public class MidiMapping()
    {
        public DJEventType DJEventType { get; set; }
        public MidiEventType MidiEventType { get; set; }
        public MidiDevice Device { get; set; } = null!;
        public int MidiChannel { get; set; }
        public int Value { get; set; }

        [JsonIgnore]
        public bool IsEnabled => Device is not null && Value > 0;

    }
}