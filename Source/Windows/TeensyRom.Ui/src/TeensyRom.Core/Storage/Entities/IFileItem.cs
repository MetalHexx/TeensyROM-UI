using System.Text.Json.Serialization;

namespace TeensyRom.Core.Storage.Entities
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
    [JsonDerivedType(typeof(SongItem), "Song")]
    [JsonDerivedType(typeof(GameItem), "Game")]
    [JsonDerivedType(typeof(HexItem), "Hex")]
    [JsonDerivedType(typeof(ImageItem), "Image")]
    [JsonDerivedType(typeof(FileItem), "File")]
    public interface IFileItem: IStorageItem
    {
        TeensyFileType FileType { get; }
        string Id { get; }
        string MetadataSource { get; set; }
        string ShareUrl { get; set; }
        string Title { get; set; }
        string Creator { get; set; }
        string ReleaseInfo { get; set; }
        string Description { get; set; }
        string Meta1 { get; set; }
        string Meta2 { get; set; }
        string MetadataSourcePath { get; set; }
        string FavChildPath { get; set; }
        string FavParentPath { get; set; }
        PlaylistItem? Custom { get; set; }

        FileItem Clone();
    }
}