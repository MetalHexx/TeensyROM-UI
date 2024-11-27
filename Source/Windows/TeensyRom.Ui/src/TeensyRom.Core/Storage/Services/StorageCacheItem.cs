using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public class StorageCacheItem
    {
        public string Path { get; set; } = string.Empty;
        public List<DirectoryItem> Directories { get; set; } = new();
        public List<IFileItem> Files { get; set; } = new();

        public StorageCacheItem() { }
        public StorageCacheItem(string path)
        {
            Path = path;
        }

        public void UpsertFile(IFileItem fileItem)
        {
            var existingFileIndex = Files.FindIndex(f => f.Name == fileItem.Name);

            if (existingFileIndex == -1)
            {
                Files.Add(fileItem);
                Files = [.. Files.OrderBy(s => s.Name)];
                return;
            }
            Files[existingFileIndex] = fileItem;
        }

        public void InsertSubdirectory(DirectoryItem directory)
        {
            var subDir = Directories.Find(d => d.Path == directory.Path);

            if (subDir is not null) return;

            Directories.Add(directory);
            Directories = [.. Directories.OrderBy(d => d.Name)];
            return;
        }

        public void DeleteFile(string path)
        {
            var fileToRemove = Files.Find(f => f.Path == path);

            if (fileToRemove is null) return;

            Files.Remove(fileToRemove);
        }

        public List<IStorageItem> ToList()
        {
            var directoryList = new List<IStorageItem>();
            directoryList.AddRange(Directories);
            directoryList.AddRange(Files);

            return directoryList;
        }
    }
}