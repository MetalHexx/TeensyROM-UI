namespace TeensyRom.Core.Entities.Storage
{
    public class YouTubeVideo 
    {
        public string VideoId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public int Subtune { get; set; }
    }
}