using System.Text.Json.Serialization;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music.Midi
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]    
    [JsonDerivedType(typeof(DualNoteMapping), "DualNoteMapping")]
    [JsonDerivedType(typeof(NoteMapping), "NoteMapping")]
    [JsonDerivedType(typeof(CCMapping), "CCMapping")]
    public abstract class MidiMapping()
    {  
        public DJEventType DJEventType { get; set; }
        public MidiDevice Device { get; set; } = new();
        public int MidiChannel { get; set; }
        public int? FilterValue { get; set; } //Could be velocity or CC value or nothing (null)
        public string DisplayName { get; set; } = string.Empty;
        public abstract bool IsEnabled { get; }
        public abstract int NoteOrCC { get; }
        public double Amount { get; set; } = 0.0;
        public bool AmountEnabled { get; set; } = false;
    }
}