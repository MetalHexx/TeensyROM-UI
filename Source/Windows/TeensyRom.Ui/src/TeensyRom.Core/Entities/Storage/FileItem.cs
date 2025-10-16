using System.Text.Json.Serialization;
using TeensyRom.Core.Common;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Entities.Storage
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
    [JsonDerivedType(typeof(SongItem), "Song")]
    [JsonDerivedType(typeof(GameItem), "Game")]
    [JsonDerivedType(typeof(HexItem), "Hex")]
    [JsonDerivedType(typeof(ImageItem), "Image")]
    [JsonDerivedType(typeof(FileItem), "File")]
    public class FileItem : StorageItem
    {
        public virtual string Title { get; set; } = string.Empty;
        public virtual string Creator { get; set; } = string.Empty;
        public virtual string ReleaseInfo { get; set; } = string.Empty;
        public virtual string Description { get; set; } = string.Empty;
        public string ShareUrl { get; set; } = string.Empty;
        public virtual string MetadataSource { get; set; } = string.Empty;
        public virtual string Meta1 { get; set; } = string.Empty;
        public virtual string Meta2 { get; set; } = string.Empty;
        public List<FileLink> Links { get; set; } = [];
        public List<FileTag> Tags { get; set; } = [];
        public List<YouTubeVideo> YouTubeVideos { get; set; } = [];
        public List<Competition> Competitions { get; set; } = [];
        public decimal? AvgRating { get; set; }
        public int RatingCount { get; set; }

        public FilePath MetadataSourcePath { get; set; } = new FilePath(string.Empty);
        public DirectoryPath ParentPath => Path.Directory;
        public PlaylistItem? Custom { get; set; } = null;
        public FilePath Path { get; set; } = new FilePath(string.Empty);
        public string Id => $"{Size}{Path.FileName}";
        public TeensyFileType FileType => Path.Extension.GetFileType();
        public bool IsCompatible { get; set; } = true;



        public virtual FileItem Clone()
        {
            return new FileItem
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
                Links = Links.Select(l => new FileLink { Name = l.Name, Url = l.Url }).ToList(),
                Tags = Tags.Select(t => new FileTag { Name = t.Name, Type = t.Type }).ToList(),
                YouTubeVideos = YouTubeVideos.Select(v => new YouTubeVideo { VideoId = v.VideoId, Url = v.Url, Channel = v.Channel, Subtune = v.Subtune }).ToList(),
                Competitions = Competitions.Select(c => new Competition { Name = c.Name, Place = c.Place }).ToList(),
                AvgRating = AvgRating,
                RatingCount = RatingCount,
                Custom = Custom?.Clone()
            };
        }
    }
}
