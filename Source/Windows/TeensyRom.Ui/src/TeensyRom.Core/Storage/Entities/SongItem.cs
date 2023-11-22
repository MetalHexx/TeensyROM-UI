using System;

namespace TeensyRom.Core.Storage.Entities
{
    public class SongItem : StorageItem
    {
        public string ArtistName { get; set; } = string.Empty;
        public string ReleaseInfo { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public TimeSpan SongLength { get; set; } = TimeSpan.FromMinutes(3);
    }
}
