using System;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Storage.Entities
{
    public class SongItem : FileItem
    {   
        public string SongName { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string ReleaseInfo { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public TimeSpan SongLength { get; set; } = TimeSpan.FromMinutes(3);

        public override SongItem Clone() => new ()
        {
            Name = Name,
            Path = Path,
            Size = Size,
            IsFavorite = IsFavorite,
            SongName = SongName,
            ArtistName = ArtistName,
            ReleaseInfo = ReleaseInfo,
            Comments = Comments            
        };
    }
}
