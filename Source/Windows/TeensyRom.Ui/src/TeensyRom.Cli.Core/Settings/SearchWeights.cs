namespace TeensyRom.Cli.Core.Settings
{
    public class SearchWeights
    {
        public double Title { get; set; } = 1;
        public double FileName { get; set; } = 0.1;
        public double FilePath { get; set; } = 0.1;
        public double Creator { get; set; } = 0.1;
        public double Description { get; set; } = 1;


    }
}