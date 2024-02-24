using System;
using System.Numerics;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Storage.Entities
{
    public class SongItem : FileItem, ILaunchableItem
    {   
        public TimeSpan SongLength { get; set; } = TimeSpan.FromMinutes(3);

        public SongItem()
        {
            Creator = "Unknown";
            ReleaseInfo = "Unknown";
            Description = "No description";
        }

        public override SongItem Clone() => new()
        {
            Name = Name,
            Path = Path,
            Size = Size,
            IsFavorite = IsFavorite,
            Title = Title,
            Creator = Creator,
            ReleaseInfo = ReleaseInfo,
            Description = Description            
        };
    }
}
