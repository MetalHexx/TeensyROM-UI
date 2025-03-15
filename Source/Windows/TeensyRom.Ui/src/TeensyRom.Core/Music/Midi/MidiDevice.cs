namespace TeensyRom.Core.Music.Midi
{
    public class MidiDevice
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public string HardwareId { get; set; } = string.Empty;
        public string ManufacturerName { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int DriverVersion { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }
}