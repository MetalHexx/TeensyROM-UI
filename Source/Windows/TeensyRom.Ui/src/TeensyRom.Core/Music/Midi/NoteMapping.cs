using System.Text.Json.Serialization;

namespace TeensyRom.Core.Music.Midi
{
    public class NoteMapping : MidiMapping 
    {   
        public int? NoteNumber { get; set; } = null;
        public NoteEventType NoteEvent { get; set; } = NoteEventType.NoteOn;
        public int? RequiredVelocity { get; set; } = null;        
        [JsonIgnore]
        public override int NoteOrCC
        {
            get
            {
                if (NoteNumber.HasValue) 
                {
                    return NoteNumber.Value;
                }
                throw new Exception("Note number must have a value!");
            }
        }
        [JsonIgnore]
        public override bool IsEnabled => Device is not null && NoteNumber is not null;
    }
}