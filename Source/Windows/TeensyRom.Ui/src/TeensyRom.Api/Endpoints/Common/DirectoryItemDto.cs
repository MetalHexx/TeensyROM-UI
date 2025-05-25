using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Common
{
    public class DirectoryItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;

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
