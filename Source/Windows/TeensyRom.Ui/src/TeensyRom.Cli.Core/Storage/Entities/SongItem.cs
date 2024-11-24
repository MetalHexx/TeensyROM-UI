using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace TeensyRom.Cli.Core.Storage.Entities
{
    public class SongItem : FileItem, ILaunchableItem, IViewableItem
    {   
        public TimeSpan PlayLength { get; set; } = TimeSpan.FromMinutes(3);
        public List<TimeSpan> SubtuneLengths { get; set; } = [];
        public int StartSubtuneNum { get; set; }
        public List<ViewableItemImage> Images { get; init; } = [];        

        public SongItem()
        {
            Creator = "Unknown";
            ReleaseInfo = "Unknown";
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
            MetadataSourcePath = MetadataSourcePath,
            PlayLength = PlayLength,
            Images = Images.Select(x => x.Clone()).ToList()
        };
    }
}
