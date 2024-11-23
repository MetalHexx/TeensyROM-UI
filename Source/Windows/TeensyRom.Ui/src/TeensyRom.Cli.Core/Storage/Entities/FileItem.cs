using TeensyRom.Cli.Core.Common;

namespace TeensyRom.Cli.Core.Storage.Entities
{
    public class FileItem : StorageItem, IFileItem
    {   
        public virtual string Title { get; set; } = string.Empty;
        public virtual string Creator { get; set; } = string.Empty;
        public virtual string ReleaseInfo { get; set; } = string.Empty;
        public virtual string Description { get; set; } = string.Empty;
        public string ShareUrl { get; set; } = string.Empty;
        public virtual string MetadataSource { get; set; } = string.Empty;
        public virtual string Meta1 { get; set; } = string.Empty;
        public virtual string Meta2 { get; set; } = string.Empty;
        public string MetadataSourcePath { get; set; } = string.Empty;
        public string FavChildPath { get; set; } = string.Empty;
        public string FavParentPath { get; set; } = string.Empty;
        public string Id => $"{Size}{Path.GetFileNameFromPath()}";
        public TeensyFileType FileType => Path.GetUnixFileExtension().GetFileType();

        

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
                MetadataSourcePath = MetadataSourcePath
            };
        }

        public static IFileItem Create(string remotePath) 
        { 
            var extension = remotePath.ToUnixPath().GetUnixFileExtension();
            var fileType = extension.GetFileType();

            return fileType switch 
            {
                TeensyFileType.Sid => new SongItem 
                {
                    Path = remotePath,
                    Name = remotePath.GetFileNameFromPath()
                },
                TeensyFileType.Prg or TeensyFileType.Crt or TeensyFileType.P00 or TeensyFileType.D64 => new GameItem
                {
                    Path = remotePath,
                    Name = remotePath.GetFileNameFromPath()
                },
                TeensyFileType.Hex => new HexItem
                {
                    Path = remotePath,
                    Name = remotePath.GetFileNameFromPath()
                },
                TeensyFileType.Kla or TeensyFileType.Koa or TeensyFileType.Art or TeensyFileType.Aas or TeensyFileType.Hpi => new ImageItem
                {
                    Path = remotePath,
                    Name = remotePath.GetFileNameFromPath()
                },
                _ => new FileItem
                {
                    Path = remotePath,
                    Name = remotePath.GetFileNameFromPath()
                }
            };
        }
    }
}
