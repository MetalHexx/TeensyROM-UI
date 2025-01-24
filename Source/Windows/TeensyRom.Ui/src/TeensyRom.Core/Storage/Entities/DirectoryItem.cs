using System.Collections.ObjectModel;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Storage.Entities
{
    public class DirectoryItem : StorageItem
    {
        public override StorageItem Clone() => new DirectoryItem 
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