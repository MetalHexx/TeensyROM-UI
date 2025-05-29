using System.ComponentModel.DataAnnotations;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Storage;

namespace TeensyRom.Api.Models
{
    /// <summary>
    /// Data transfer object representing a cached directory in TeensyROM storage, including its files and subdirectories.
    /// </summary>
    public class StorageCacheDto
    {
        /// <summary>
        /// The list of subdirectories in the cached directory.
        /// </summary>
        [Required] public List<DirectoryItemDto> Directories { get; set; } = [];

        /// <summary>
        /// The list of files in the cached directory.
        /// </summary>
        [Required] public List<FileItemDto> Files { get; set; } = [];

        /// <summary>
        /// The full path to the cached directory.
        /// </summary>
        [Required] public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Creates a <see cref="StorageCacheDto"/> from an <see cref="IStorageCacheItem"/> entity.
        /// </summary>
        public static StorageCacheDto FromCache(IStorageCacheItem cache)
        {
            return new ()
            {
                Path = cache.Path,
                Directories = [.. cache.Directories
                    .Select(d => new DirectoryItemDto
                    {
                        Name = d.Name,
                        Path = d.Path
                    })],
                Files = [.. cache.Files
                    .OfType<ILaunchableItem>()
                    .Select(FileItemDto.FromLaunchable)]
            };
        }
    }
}
