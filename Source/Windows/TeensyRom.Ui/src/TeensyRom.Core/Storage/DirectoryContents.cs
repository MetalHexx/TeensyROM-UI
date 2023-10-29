using Newtonsoft.Json;

namespace TeensyRom.Core.Storage
{
    public class DirectoryContent
    {
        public string Path { get; set; } = string.Empty;
        public int TotalCount => Directories.Count() + Files.Count();
        public List<DirectoryItem> Directories { get; set; } = new();
        public List<FileItem> Files { get; set; } = new();

        public void Add(DirectoryContent content)
        {
            Directories.AddRange(content.Directories);
            Files.AddRange(content.Files);
        }
    }

    public class DirectoryItem
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public class FileItem
    {
        public string Name { get; set; } = string.Empty;
        public int Size { get; set; }
        public string Path { get; set; } = string.Empty;
    }
}