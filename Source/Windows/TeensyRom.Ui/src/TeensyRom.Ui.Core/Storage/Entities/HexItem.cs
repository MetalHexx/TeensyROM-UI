
using System;
using TeensyRom.Ui.Core.Games;

namespace TeensyRom.Ui.Core.Storage.Entities
{
    public class HexItem : FileItem, ILaunchableItem, IViewableItem
    {
        public override string Creator => "Firmware Update";
        public override string Title => $"{Name[..Name.LastIndexOf('.')]}";
        public override string Meta1 => "Travis Smith";
        public override string Meta2 => "Sensorium Embedded";
        public override string Description => "Launch this to update your TeensyROM cartridge with a new firmware version.\r\rAfter launching, look at the C64 display for further instructions.";

        public List<ViewableItemImage> Images { get; init; } = [];

        public override HexItem Clone() => new()
        {
            Name = Name,
            Path = Path,
            Size = Size,
            IsFavorite = IsFavorite,
            ReleaseInfo = ReleaseInfo,
            ShareUrl = ShareUrl,
            MetadataSource = MetadataSource,
            MetadataSourcePath = MetadataSourcePath,
            Images = Images.Select(x => x.Clone()).ToList()
        };
    }
}
