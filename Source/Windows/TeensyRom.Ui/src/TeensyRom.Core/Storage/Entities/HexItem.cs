
using System;
using TeensyRom.Core.Assets;
using TeensyRom.Core.Games;

namespace TeensyRom.Core.Storage.Entities
{
    public class HexItem : FileItem, ILaunchableItem, IViewableItem
    {
        public override string Creator => "Firmware Update";
        public override string Title => $"{Name[..Name.LastIndexOf('.')]}";
        public override string Meta1 => "Travis Smith";
        public override string Meta2 => "Sensorium Embedded";
        public override string Description => "Launch this to update your TeensyROM cartridge with a new firmware version.\r\rAfter launching, look at the C64 display for further instructions.";

        public List<ViewableItemImage> Images { get; init; } = [];

        public HexItem()
        {
            MapImages();
        }

        private void MapImages() 
        {
            File.ReadAllText(GameConstants.Game_Image_Metadata_File_Path);
            var hardwareFileInfo = new FileInfo(AssetConstants.TeensyRomHardwareFilePath);

            Images.Add(new ViewableItemImage
            {
                FileName = hardwareFileInfo.Name,
                Path = hardwareFileInfo.FullName,
                Source = "SensoriumEmbedded"
            });
        }
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
