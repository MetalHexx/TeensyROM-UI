namespace TeensyRom.Core.Entities.Storage
{
    public class SongItem : LaunchableItem, IViewableItem
    {
        public TimeSpan PlayLength { get; set; } = TimeSpan.FromMinutes(3);
        public List<TimeSpan> SubtuneLengths { get; set; } = [];
        public int StartSubtuneNum { get; set; }
        public string SongCsdbUrl { get; set; } = string.Empty;
        public List<ViewableItemImage> Images { get; init; } = [];

        public bool IsPlaylistedOrFavorite => Path.Value.Contains(StorageHelper.Playlist_Path) || Path.ToString().Contains(StorageHelper.Favorites_Path);

        public SongItem()
        {
            Creator = "";
            ReleaseInfo = "";
            Description = "";
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
            Images = Images.Select(x => x.Clone()).ToList(),
            Custom = Custom?.Clone()
        };
    }
}
