using System.Text.Json.Serialization;

namespace TeensyRom.Core.Music.Midi
{
    public class CCMapping : MidiMapping 
    {
        public int? CCNumber { get; set; } = null;
        public int? RequiredValue { get; set; } = null;
        public CCType CCType { get; set; } = CCType.Relative1;

        [JsonIgnore]
        public override int NoteOrCC
        {
            get
            {
                if (CCNumber.HasValue)
                {
                    return CCNumber.Value;
                }
                throw new Exception("CC number must have a value!");
            }
        }
        [JsonIgnore]
        public override bool IsEnabled => Device is not null && CCNumber is not null;
    }
}