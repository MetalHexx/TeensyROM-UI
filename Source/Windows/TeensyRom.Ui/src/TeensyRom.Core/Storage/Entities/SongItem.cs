using System;
using System.Numerics;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Storage.Entities
{
    public class SongItem : FileItem, ILaunchableItem, IViewableItem
    {   
        public TimeSpan SongLength { get; set; } = TimeSpan.FromMinutes(3);
        public List<ViewableItemImage> Images { get; init; } = [];        

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
            Description = Description,
            ShareUrl = ShareUrl,
            MetadataSource = MetadataSource,
            Meta1 = Meta1,
            Meta2 = Meta2,
            SourcePath = SourcePath,
            SongLength = SongLength,
            Images = Images.Select(x => x.Clone()).ToList()
        };
    }
}
