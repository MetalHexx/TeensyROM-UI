using Newtonsoft.Json;

namespace TeensyRom.Cli.Core.Storage.Entities
{
    public class DirectoryContent
    {
        public string Path { get; set; } = string.Empty;
        public int TotalCount => Directories.Count() + Files.Count();
        public List<DirectoryItem> Directories { get; set; } = new();
        public List<IFileItem> Files { get; set; } = new();

        public void Add(DirectoryContent content)
        {
            Directories.AddRange(content.Directories);
            Files.AddRange(content.Files);
        }
    }
}