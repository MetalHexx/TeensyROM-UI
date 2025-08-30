using System.Collections.ObjectModel;
using TeensyRom.Core.Common;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Entities.Storage
{
    public class DirectoryItem : StorageItem
    {
        public new string Name => Path.DirectoryName;
        public DirectoryPath Path { get; set; } = new DirectoryPath(string.Empty);

        public DirectoryItem() { }

        public DirectoryItem(string directoryPath) 
        {
            Path = new DirectoryPath(directoryPath);
        }

        public DirectoryItem(DirectoryPath path)
        {
            Path = path;
        }
    }
}