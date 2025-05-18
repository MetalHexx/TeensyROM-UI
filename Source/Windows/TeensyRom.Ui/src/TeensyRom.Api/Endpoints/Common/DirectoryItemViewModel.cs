using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Common
{
    public class DirectoryItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;

        public static DirectoryItemViewModel FromDirectory(DirectoryItem item)
        {
            return new DirectoryItemViewModel
            {
                Name = item.Name,
                Path = item.Path,
            };
        }
    }
}
