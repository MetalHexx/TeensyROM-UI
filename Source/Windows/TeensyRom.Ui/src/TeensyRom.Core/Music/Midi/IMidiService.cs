
namespace TeensyRom.Core.Music.Midi
{
    public interface IMidiService
    {
        IObservable<MidiEvent> MidiEvents { get; }

        Task<MidiResult?> GetFirstMidiEvent(MidiEventType eventType);
        IEnumerable<MidiDevice> GetMidiDevices();
        void ReceiveMidiInputLoop();
        void EngageMidi(MidiSettings midiSettings);
        void SendRandomMidiNotes(int deviceId, int durationMs = 200, int count = 50);
        void SendRandomMidiNotesToAllDevices();
        void DisengageMidi();
    }
}