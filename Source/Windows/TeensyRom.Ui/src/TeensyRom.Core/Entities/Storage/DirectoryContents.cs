using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Entities.Storage
{
    public class DirectoryContent
    {
        public DirectoryPath Path { get; set; } = new DirectoryPath(string.Empty);
        public int TotalCount => Directories.Count() + Files.Count();
        public List<DirectoryItem> Directories { get; set; } = [];
        public List<FileItem> Files { get; set; } = [];

        public List<DirectoryItem> MapAndOrderDirectories()
        {
            return Directories
                .Select(d => new DirectoryItem(d.Path))
                .OrderBy(d => d.Name)
                .ToList() ?? [];
        }
    }
}