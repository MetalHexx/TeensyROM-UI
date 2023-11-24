using System.Collections.ObjectModel;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Storage.Entities
{
    public class DirectoryItem : StorageItem
    {

        public ObservableCollection<DirectoryItem> Directories { get; set; } = new ObservableCollection<DirectoryItem>();

        public void Insert(IEnumerable<DirectoryItem> newDirectories)
        {
            if (!newDirectories.Any()) return;

            var parentPath = newDirectories.First().Path
                .GetParentDirectory()
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            var currentPath = Path
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            if (currentPath.Equals(parentPath))
            {
                AddDirectories(newDirectories);
                return;
            }
            RecurseDirectories(newDirectories);
            return;
        }

        private void RecurseDirectories(IEnumerable<DirectoryItem> newDirectories)
        {
            foreach (var directory in Directories)
            {
                directory.Insert(newDirectories);
            }
        }

        private void AddDirectories(IEnumerable<DirectoryItem> directories)
        {

            foreach (var directory in directories)
            {
                var notDupe = !Directories.Any(d => d.Path == directory.Path);

                if (notDupe)
                {
                    Directories.Add(new DirectoryItem()
                    {
                        Name = directory.Name,
                        Path = directory.Path
                    });
                }
            };            
        }
    }
}