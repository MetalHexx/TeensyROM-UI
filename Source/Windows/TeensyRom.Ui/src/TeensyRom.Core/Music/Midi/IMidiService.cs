
namespace TeensyRom.Core.Music.Midi
{
    public interface IMidiService
    {
        IObservable<MidiEvent> MidiEvents { get; }
        bool IsMidiEngaged { get; }

        Task<MidiResult?> GetFirstMidiEvent(MidiEventType eventType);
        IEnumerable<MidiDevice> GetMidiDevices();
        void EngageMidi(MidiSettings midiSettings);
        void DisengageMidi();
        void EngageMidi();
        void Dispose();
    }
}