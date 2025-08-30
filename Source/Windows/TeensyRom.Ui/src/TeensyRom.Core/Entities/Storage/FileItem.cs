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
        public FilePath MetadataSourcePath { get; set; } = new FilePath(string.Empty);
        public FilePath ParentPath { get; set; } = new FilePath(string.Empty);
        public PlaylistItem? Custom { get; set; } = null;
        public FilePath Path { get; set; } = new FilePath(string.Empty);
        public string Id => $"{Size}{Path.FileName}";
        public TeensyFileType FileType => Path.Extension.GetFileType();



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
                Custom = Custom?.Clone()
            };
        }
    }
}
