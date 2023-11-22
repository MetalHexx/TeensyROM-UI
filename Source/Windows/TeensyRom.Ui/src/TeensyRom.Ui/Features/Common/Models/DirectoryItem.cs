using System.Collections.Generic;
using System.Linq;
using TeensyRom.Core.Common;

namespace TeensyRom.Ui.Features.Common.Models
{
    public class DirectoryItemVm : StorageItemVm 
    {
        public List<DirectoryItemVm> Directories { get; set; } = new List<DirectoryItemVm>();

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
                    .Select(d => new DirectoryItemVm()
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
