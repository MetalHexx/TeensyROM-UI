namespace TeensyRom.Core.Entities.Storage
{
    public class DirectoryContent
    {
        public string Path { get; set; } = string.Empty;
        public int TotalCount => Directories.Count() + Files.Count();
        public List<DirectoryItem> Directories { get; set; } = [];
        public List<IFileItem> Files { get; set; } = [];

        public List<DirectoryItem> MapAndOrderDirectories()
        {
            return Directories
                .Select(d => new DirectoryItem
                {
                    Name = d.Name,
                    Path = d.Path
                })
                .OrderBy(d => d.Name)
                .ToList() ?? [];
        }
    }
}