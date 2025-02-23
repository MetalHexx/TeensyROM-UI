namespace TeensyRom.Core.Music.Midi
{
    public class MidiSettings 
    {
        public MidiMapping PlayPause { get; set; } = new() 
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.PlayPause,
        };
        public MidiMapping Stop { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.Stop,
        };
        public MidiMapping Next { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.Next,
        };
        public MidiMapping Previous { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.Previous,
        };
        public MidiMapping Seek { get; set; } = new()
        {
            MidiEventType = MidiEventType.ControlChange,
            DJEventType = DJEventType.Seek,
        };
        public MidiMapping FastForward { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.FastForward,
        };
        public MidiMapping NudgeForward { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.NudgeForward,
        };
        public MidiMapping NudgeBackward { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.NudgeBackward,
        };
        public MidiMapping CurrentSpeed { get; set; } = new() 
        {
            MidiEventType = MidiEventType.ControlChange,
            DJEventType = DJEventType.CurrentSpeed,
        };
        public MidiMapping CurrentSpeedFine { get; set; } = new()
        {
            MidiEventType = MidiEventType.ControlChange,
            DJEventType = DJEventType.CurrentSpeedFine,
        };
        public MidiMapping SetSpeedPlus50 { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.SetSpeedPlus50,
        };
        public MidiMapping SetSpeedMinus50 { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.SetSpeedMinus50,
        };
        public MidiMapping HomeSpeed { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.HomeSpeed,
        };
        public MidiMapping Voice1 { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.Voice1,
        };
        public MidiMapping Voice2 { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.Voice2,
        };
        public MidiMapping Voice3 { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.Voice3,
        };

        public MidiMapping Mode { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteOff,
            DJEventType = DJEventType.Mode,
        };

        public bool SnapToSpeed { get; set; } = false;
        public bool SnapToSeek { get; set; } = false;
    }
}