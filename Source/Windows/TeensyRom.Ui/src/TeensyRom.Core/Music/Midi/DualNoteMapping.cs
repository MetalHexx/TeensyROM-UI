namespace TeensyRom.Core.Music.Midi
{
    public class DualNoteMapping : NoteMapping
    {
        public NoteEventType NoteEvent2 { get; set; } = NoteEventType.NoteOff;
    }
}