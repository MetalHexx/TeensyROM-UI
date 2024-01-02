using System.Collections.Generic;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public class StorageCacheItem
    {
        public string Path { get; set; } = string.Empty;
        public List<DirectoryItem> Directories { get; set; } = new();
        public List<FileItem> Files { get; set; } = new();

        public void UpsertFile(FileItem fileItem)
        {
            var fileIndex = Files.IndexOf(fileItem);

            if (fileIndex == -1)
            {
                Files.Add(fileItem);
                Files = [.. Files.OrderBy(s => s.Name)];
                return;
            }
            Files[fileIndex] = fileItem;
        }
    }
}