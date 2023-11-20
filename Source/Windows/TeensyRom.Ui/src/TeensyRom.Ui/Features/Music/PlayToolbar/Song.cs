using System;

namespace TeensyRom.Ui.Features.Music.PlayToolbar
{
    public class Song
    {
        public string Path { get; set; } = string.Empty;        
        public string ArtistName { get; set; } = string.Empty;
        public string SongName { get; set; } = string.Empty;
        public TimeSpan SongLength { get; set; } = new TimeSpan(0, 5, 0);        
    }
}
