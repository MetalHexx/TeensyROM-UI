
namespace TeensyRom.Core.Storage.Entities
{
    public class GameItem : FileItem, ILaunchableItem, IViewableItem
    {
        public List<ViewableItemImage> Images { get; init; } = [];

        public override GameItem Clone() => new ()
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
            Images = Images.Select(x => x.Clone()).ToList()
        };
    }
}
