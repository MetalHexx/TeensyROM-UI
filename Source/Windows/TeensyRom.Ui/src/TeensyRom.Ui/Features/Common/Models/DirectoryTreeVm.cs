using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using TeensyRom.Core.Common;

namespace TeensyRom.Ui.Features.Common.Models
{
    public class DirectoryTreeVm: StorageItemVm
    {
        public List<DirectoryTreeVm> Directories { get; set; } = new List<DirectoryTreeVm>();

        public void AddRange(IEnumerable<DirectoryItemVm> newDirectories)
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
                    .Select(d => new DirectoryTreeVm()
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
