namespace TeensyRom.Core.Storage.Entities
{
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

        FileItem Clone();
    }
}