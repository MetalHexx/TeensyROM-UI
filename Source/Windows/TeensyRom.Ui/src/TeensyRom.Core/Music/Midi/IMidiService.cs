namespace TeensyRom.Core.Music.Midi
{
    public interface IMidiService
    {
        IObservable<MidiEvent> MidiEvents { get; }
        IEnumerable<MidiDevice> GetMidiDevices();
        void ReceiveMidiInputLoop();
        void SendRandomMidiNotes(int deviceId, int durationMs = 200, int count = 50);
        void SendRandomMidiNotesToAllDevices();
    }
}