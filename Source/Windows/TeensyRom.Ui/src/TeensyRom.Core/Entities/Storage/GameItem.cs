namespace TeensyRom.Core.Entities.Storage
{
    public class GameItem : FileItem, ILaunchableItem, IViewableItem
    {
        public override string Creator => GetExtensionShortDescription();
        public override string Description => GetExtensionLongDescription();
        public override string Title => $"{Name[..Name.LastIndexOf('.')]}";
        public override string Meta1 => Name[(Name.LastIndexOf('.') + 1)..];
        public List<ViewableItemImage> Images { get; init; } = [];
        public TimeSpan PlayLength { get; set; } = TimeSpan.FromMinutes(3);

        private string GetExtensionShortDescription()
        {
            return FileType switch
            {
                TeensyFileType.Crt => "Cartridge Image",
                TeensyFileType.Prg => "Program File",
                TeensyFileType.P00 => "Emulator Program File",
                _ => "Unknown File Type"
            };
        }


        private string GetExtensionLongDescription()
        {
            return FileType switch
            {
                TeensyFileType.Crt => "About CRT Files:\r\rThis file represents a cartridge image used with Commodore 64 emulators. The CRT format is a container for raw cartridge data, including both ROM content and the necessary metadata. Cartridges were used for games, utilities, and other applications, providing a plug-and-play experience in an era before the prevalence of disk drives.",
                TeensyFileType.Prg => "About PRG Files:\r\rA program file format used by Commodore computers. PRG files contain executable programs, typically storing both the data and the necessary code to start the program automatically upon loading. These files were commonly used for software distribution and are a quintessential part of the C64 software library, ranging from games to productivity applications.",
                TeensyFileType.P00 => "About P00 Files:\r\rUnique to Commodore emulation, the P00 format is a preservation of PRG files, prefixed with a file header that includes the original filename and other metadata. This format is used by emulators to accurately recreate the experience of running C64 software, ensuring that legacy programs can be enjoyed by future generations with authenticity.",
                _ => "Unknown File Type"
            };
        }


        public override GameItem Clone() => new()
        {
            Name = Name,
            Path = Path,
            Size = Size,
            IsFavorite = IsFavorite,
            Title = Title,
            Creator = Creator,
            ReleaseInfo = ReleaseInfo,
            Description = Description,
            ShareUrl = ShareUrl,
            MetadataSource = MetadataSource,
            Meta1 = Meta1,
            Meta2 = Meta2,
            MetadataSourcePath = MetadataSourcePath,
            Images = Images.Select(x => x.Clone()).ToList(),
            Custom = Custom?.Clone()
        };
    }
}
