namespace TeensyRom.Core.Music.Midi
{
    public record MidiEvent(DJEventType DJEventType, MidiEventType MidiEventType, MidiMapping mapping, int Value);
}