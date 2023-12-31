using System.Collections.Generic;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public class FileDirectory
    {
        public string Path { get; set; } = string.Empty;
        public List<DirectoryItem> Directories { get; set; } = new();
        public List<FileItem> Files { get; set; } = new();

        public void UpsertFile(FileItem fileItem)
        {
            var fileIndex = Files.IndexOf(fileItem);

            if (fileIndex == -1)
            {
                Insert(fileItem);
                return;
            }
            Files[fileIndex] = fileItem;
        }

        public void Upsert(DirectoryItem directory)
        {
            var dirIndex = Directories.IndexOf(directory);

            if (dirIndex == -1)
            {
                Insert(directory);
                return;
            }
            Directories[dirIndex] = directory;
        }

        public void Insert(DirectoryItem directory)
        {
            Directories.Add(directory);
            Directories = [.. Directories.OrderBy(s => s.Name)];
        }

        public void Insert(FileItem song)
        {
            Files.Add(song);
            Files = [.. Files.OrderBy(s => s.Name)];
        }
    }
}