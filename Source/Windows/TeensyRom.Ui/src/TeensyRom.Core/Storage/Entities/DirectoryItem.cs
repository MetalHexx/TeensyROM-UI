using System.Collections.Generic;
using System.Linq;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Storage.Entities
{
    public class DirectoryItem : StorageItem 
    {
        public List<DirectoryItem> Directories { get; set; } = new List<DirectoryItem>();

        public void AddRange(IEnumerable<DirectoryItem> newDirectories)
        {
            var exampleDirectory = newDirectories.FirstOrDefault();

            if (exampleDirectory is null) return;

            var trimmedParentPath = exampleDirectory.Path
                .GetParentDirectory()
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            var trimmedCurrentPath = Path
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            if (trimmedCurrentPath.Equals(trimmedParentPath))
            {
                Directories.AddRange(newDirectories
                    .OrderBy(d => d.Name)
                    .Select(d => new DirectoryItem()
                    {
                        Name = d.Name,
                        Path = d.Path
                    }));

                return;
            }
            foreach (var directory in Directories)
            {
                directory.AddRange(newDirectories);
            }
            return;
        }
    }
}
