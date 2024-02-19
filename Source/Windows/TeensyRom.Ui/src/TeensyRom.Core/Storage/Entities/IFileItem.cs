namespace TeensyRom.Core.Storage.Entities
{
    public interface IFileItem: IStorageItem
    {
        TeensyFileType FileType { get; }
        string Id { get; }
        string MetadataSource { get; set; }
        string ShareUrl { get; set; }

        FileItem Clone();
    }
}