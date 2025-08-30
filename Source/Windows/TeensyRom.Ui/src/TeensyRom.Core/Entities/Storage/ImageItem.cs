using System.Reflection;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Entities.Storage
{
    public class ImageItem : LaunchableItem
    {
        public override string Creator => GetExtensionShortDescription();
        public override string Description => GetExtensionLongDescription();
        public override string Title => $"{Name[..Name.LastIndexOf('.')]}";
        public override string Meta1 => Name[(Name.LastIndexOf('.') + 1)..];
        public TimeSpan PlayLength { get; set; } = TimeSpan.FromMinutes(1);

        private string GetExtensionShortDescription()
        {
            return FileType switch
            {
                TeensyFileType.Kla => "Koala Painter Image",
                TeensyFileType.Koa => "Koala Painter Image",
                TeensyFileType.Art => "Art Studio Image",
                TeensyFileType.Aas => "Advanced Art Studio Image",
                TeensyFileType.Hpi => "Hi-Pic Creator Image",
                TeensyFileType.Seq => "Sequential Data",
                TeensyFileType.Txt => "Text File",
                _ => "Unknown File Type"
            };
        }

        private string GetExtensionLongDescription()
        {
            return FileType switch
            {
                TeensyFileType.Kla => "About KLA Files:\r\rA bitmap graphics file for the Commodore 64, created with the KoalaPainter software. Characterized by its 160x200 pixel resolution and 16-color palette, Koala files encapsulate the vibrant pixel art era of the 1980s and are notable for their widespread use alongside the KoalaPad peripheral.",
                TeensyFileType.Koa => "About KOA Files:\r\rIdentical to KLA, the KOA format hails from the same-named KoalaPainter software. With a resolution of 160x200 and a 16-color palette, it stands as a hallmark of C64 artistry, easily recognizable by its chunky pixels and vivid color schemes reflecting the creative potential of early computer art.",
                TeensyFileType.Art => "About ART Files:\r\rA file format from Art Studio, a prominent bitmap graphics editor on the C64. It features a resolution of 160x200 pixels using a 16-color palette and was celebrated for bringing detailed pixel graphics editing to the home user, showcasing the graphic capabilities of the C64.",
                TeensyFileType.Aas => "About AAS Files:\r\rThe successor to Art Studio, the AAS format comes from Advanced Art Studio, pushing the C64's graphical prowess further. It maintained the standard 160x200 resolution with a 16-color palette but introduced enhanced tools and features for the burgeoning community of home computer artists.",
                TeensyFileType.Hpi => "About HPI Files:\r\rA less common but technically intriguing format used by Hi-Pic Creator, a C64 program designed for high-resolution images. HPI files often pushed beyond standard C64 limitations, experimenting with resolutions and color techniques that paved the way for future graphics innovations on home computers.",
                TeensyFileType.Seq => "About SEQ Files:\r\rA sequential data file type widely used on the Commodore 64. Encoded in PETSCII, SEQ files are popular in the C64 scene for storing structured text, game data, or custom tools. They reflect the retro computing era's focus on creative data handling and continue to play a role in modern retro development.",
                TeensyFileType.Txt => "About TXT Files:\r\rA plain text file format that stores readable text, typically encoded in PETSCII on the Commodore 64. TXT files are versatile and widely used for documentation, programming notes, and script storage. Their simplicity makes them ideal for use across retro systems and modern workflows alike.",
                _ => "Unknown File Type"
            };
        }
        public override ImageItem Clone() => new()
        {
            Name = Name,
            Path = Path,
            Size = Size,
            PlayLength = PlayLength,
            IsFavorite = IsFavorite,
            ReleaseInfo = ReleaseInfo,
            ShareUrl = ShareUrl,
            MetadataSource = MetadataSource,
            Meta2 = Meta2,
            MetadataSourcePath = MetadataSourcePath,
            Custom = Custom?.Clone()
        };
    }
}
