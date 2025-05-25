using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Storage;

namespace TeensyRom.Api.Endpoints.Common
{
    public class StorageCacheDto
    {
        public List<DirectoryItemDto> Directories { get; set; } = [];
        public List<FileItemDto> Files { get; set; } = [];
        public string Path { get; set; } = string.Empty;

        public static StorageCacheDto FromCache(IStorageCacheItem cache)
        {
            return new ()
            {
                Path = cache.Path,
                Directories = cache.Directories
                    .Select(d => new DirectoryItemDto
                    {
                        Name = d.Name,
                        Path = d.Path
                    })
                    .ToList(),
                Files = cache.Files
                    .OfType<ILaunchableItem>()
                    .Select(FileItemDto.FromLaunchable)
                    .ToList()
            };
        }
    }
}
