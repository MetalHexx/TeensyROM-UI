using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Models
{
    /// <summary>
    /// Data transfer object representing a directory item in TeensyROM storage.
    /// </summary>
    public class DirectoryItemDto
    {
        /// <summary>
        /// The name of the directory.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The full path to the directory.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Creates a <see cref="DirectoryItemDto"/> from a <see cref="DirectoryItem"/> entity.
        /// </summary>
        public static DirectoryItemDto FromDirectory(DirectoryItem item)
        {
            return new DirectoryItemDto
            {
                Name = item.Name,
                Path = item.Path,
            };
        }
    }
}
