using TeensyRom.Core.Entities.Midi;

namespace TeensyRom.Core.Midi
{
    public record MidiEvent(DJEventType DJEventType, MidiEventType MidiEventType, MidiMapping Mapping, int Value);
}