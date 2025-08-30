using System.Collections;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Storage
{
    public class StorageCacheItem : IStorageCacheItem
    {
        public DirectoryPath Path { get; set; }
        public List<DirectoryItem> Directories { get; set; } = new();
        public List<FileItem> Files { get; set; } = new();

        public StorageCacheItem() { }
        public StorageCacheItem(string path)
        {
            Path = new DirectoryPath(path);
        }

        public void UpsertFile(FileItem fileItem)
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
            var subDir = Directories.Find(d => d.Path.Equals(directory.Path));

            if (subDir is not null) return;

            Directories.Add(directory);
            Directories = [.. Directories.OrderBy(d => d.Name)];
            return;
        }

        public void DeleteFile(FilePath path)
        {
            var fileToRemove = Files.Find(f => f.Path.Equals(path));

            if (fileToRemove is null) return;

            Files.Remove(fileToRemove);
        }

        public List<StorageItem> ToList()
        {
            var directoryList = new List<StorageItem>();
            directoryList.AddRange(Directories);
            directoryList.AddRange(Files);

            return directoryList;
        }
    }
}