using Spectre.Console;

namespace TeensyRom.Cli.Helpers
{
    public record MarkupColor(Color Color, string Name)
    {
        public override string ToString()
        {
            return Name;
        }
    }
}
