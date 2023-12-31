using System;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Storage.Entities
{
    public class SongItem : FileItem
    {
        public string Id => $"{Size}{Path.GetFileNameFromPath()}";
        public string ArtistName { get; set; } = string.Empty;
        public string ReleaseInfo { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public TimeSpan SongLength { get; set; } = TimeSpan.FromMinutes(3);

        public SongItem Clone() => new SongItem
        {
            ArtistName = ArtistName,
            Comments = Comments,
            IsFavorite = IsFavorite,
            Name = Name,
            ReleaseInfo = ReleaseInfo
        };
    }
}
