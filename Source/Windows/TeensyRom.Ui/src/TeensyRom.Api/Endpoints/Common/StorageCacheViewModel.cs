using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Storage;

namespace TeensyRom.Api.Endpoints.Common
{
    public class StorageCacheViewModel
    {
        public List<DirectoryItemViewModel> Directories { get; set; } = new();
        public List<FileItemViewModel> Files { get; set; } = new();
        public string Path { get; set; } = string.Empty;

        public static StorageCacheViewModel FromCache(IStorageCacheItem cache)
        {
            return new StorageCacheViewModel
            {
                Path = cache.Path,
                Directories = cache.Directories
                    .Select(d => new DirectoryItemViewModel
                    {
                        Name = d.Name,
                        Path = d.Path
                    })
                    .ToList(),
                Files = cache.Files
                    .OfType<ILaunchableItem>()
                    .Select(FileItemViewModel.FromLaunchable)
                    .ToList()
            };
        }
    }
}
