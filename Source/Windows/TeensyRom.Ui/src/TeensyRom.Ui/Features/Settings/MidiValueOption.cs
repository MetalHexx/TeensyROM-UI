namespace TeensyRom.Ui.Features.Settings
{
    public class MidiValueOption
    {
        public int? Value { get; set; }
        public string DisplayText => Value?.ToString() ?? "---";

        public static MidiValueOption NullOption => new() { Value = null };
    }
}