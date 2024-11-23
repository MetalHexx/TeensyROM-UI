using System.Collections.ObjectModel;
using TeensyRom.Cli.Core.Common;

namespace TeensyRom.Cli.Core.Storage.Entities
{
    public class DirectoryItem : StorageItem 
    {
        public DirectoryItem Clone() => new DirectoryItem
        {
            IsCompatible = IsCompatible,
            IsFavorite = IsFavorite,
            IsSelected = IsSelected,
            Name = Name,
            Path = Path,
            Size = Size
        };
    }
}