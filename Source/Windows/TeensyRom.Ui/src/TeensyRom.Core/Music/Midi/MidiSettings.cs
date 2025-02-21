namespace TeensyRom.Core.Music.Midi
{
    public class MidiSettings 
    {
        public MidiMapping PlayPause { get; set; } = new() 
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.PlayPause,
        };
        public MidiMapping Stop { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.Stop,
        };
        public MidiMapping Next { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.Next,
        };
        public MidiMapping Previous { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.Previous,
        };
        public MidiMapping Seek { get; set; } = new()
        {
            MidiEventType = MidiEventType.ControlChange,
            DJEventType = DJEventType.Seek,
        };
        public MidiMapping FastForward { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.FastForward,
        };
        public MidiMapping NudgeFoward { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.NudgeFoward,
        };
        public MidiMapping NudgeBack { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.NudgeBack,
        };
        public MidiMapping CurrentSpeed { get; set; } = new() 
        {
            MidiEventType = MidiEventType.ControlChange,
            DJEventType = DJEventType.CurrentSpeed,
        };    
        public MidiMapping SetSpeedPlus50 { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.SetSpeedPlus50,
        };
        public MidiMapping SetSpeedMinus50 { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.SetSpeedMinus50,
        };
        public MidiMapping HomeSpeed { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.HomeSpeed,
        };
        public MidiMapping Voice1 { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.Voice1,
        };
        public MidiMapping Voice2 { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.Voice2,
        };
        public MidiMapping Voice3 { get; set; } = new()
        {
            MidiEventType = MidiEventType.NoteChange,
            DJEventType = DJEventType.Voice3,
        };
        public bool SnapToSpeed { get; set; } = false;
        public bool SnapToSeek { get; set; } = false;
    }
}