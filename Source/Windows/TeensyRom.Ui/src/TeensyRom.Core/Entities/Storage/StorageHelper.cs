using System.Collections.Immutable;
using TeensyRom.Core.Settings;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Entities.Storage
{
    public static class StorageHelper
    {
        public const string CachePath = @"Assets\System\Cache\";
        public const string Usb_Cache_File_Relative_Path = @"Assets\System\Cache\";
        public const string Sd_Cache_File_Relative_Path = @"Assets\System\Cache\";
        public const string Usb_Cache_File_Name = "Usb-";
        public const string Sd_Cache_File_Name = "Sd-";
        public const string Cache_File_Extension = ".json";
        public const string Remote_Path_Root = @"/";
        public const string Extraction_Path = @"Extraction\Output";
        public const string Playlist_Path = "/playlists/";
        public const string Favorites_Path = "/favorites/";
        public const string Firmware_Path = "/firmware/";
        public const string Playlist_File_Name = "playlist.json";
        public const string Temp_Path = @"Temp";
        public static readonly ImmutableList<TeensyTarget> FileTargets =
        [
            new(TeensyFileType.Sid, TeensyFilterType.Music, "SID", ".sid"),
            new(TeensyFileType.Prg, TeensyFilterType.Games, "PRG", ".prg"),
            new(TeensyFileType.P00, TeensyFilterType.Games, "P00", ".p00"),
            new(TeensyFileType.Crt, TeensyFilterType.Games, "CRT", ".crt"),
            new(TeensyFileType.Hex, TeensyFilterType.Hex, "HEX", ".hex"),
            new(TeensyFileType.Kla, TeensyFilterType.Images, "KLA", ".kla"),
            new(TeensyFileType.Koa, TeensyFilterType.Images, "KOA", ".koa"),
            new(TeensyFileType.Art, TeensyFilterType.Images, "ART", ".art"),
            new(TeensyFileType.Aas, TeensyFilterType.Images, "AAS", ".aas"),
            new(TeensyFileType.Hpi, TeensyFilterType.Images, "HPI", ".hpi"),
            new(TeensyFileType.Seq, TeensyFilterType.Images, "SEQ", ".seq"),
            new(TeensyFileType.Txt, TeensyFilterType.Images, "TXT", ".txt"),
            new(TeensyFileType.D64, TeensyFilterType.Images, "NFO", ".nfo"),
            new(TeensyFileType.D64, TeensyFilterType.Games, "D64", ".d64"),            

        ];
        public static readonly ImmutableList<TeensyFilter> Filters =
        [
            new(TeensyFilterType.All, "All", "AllInclusive"),
            new(TeensyFilterType.Music, "Music", "MusicClefTreble"),
            new(TeensyFilterType.Games, "Games", "Ghost"),
            new(TeensyFilterType.Hex, "Hex", "ArrowUpBoldHexagonOutline"),
            new(TeensyFilterType.Images, "Images", "FileImageOutline")
        ];

        public static ImmutableList<DirectoryPath> FavoritePaths => FileTargets
            .Select(t => GetFavoritePath(t.Type))
            .Distinct()
            .ToImmutableList();

        public static DirectoryPath GetFavoritePath(TeensyFileType type)
        {
            return type switch
            {
                TeensyFileType.Sid => new DirectoryPath($"{Favorites_Path}music"),
                TeensyFileType.Prg => new DirectoryPath($"{Favorites_Path}games"),
                TeensyFileType.P00 => new DirectoryPath($"{Favorites_Path}games"),
                TeensyFileType.Crt => new DirectoryPath($"{Favorites_Path}games"),
                TeensyFileType.Txt => new DirectoryPath($"{Favorites_Path}text"),
                TeensyFileType.Nfo => new DirectoryPath($"{Favorites_Path}text"),
                TeensyFileType.Seq => new DirectoryPath($"{Favorites_Path}images"),
                TeensyFileType.Kla => new DirectoryPath($"{Favorites_Path}images"),
                TeensyFileType.Koa => new DirectoryPath($"{Favorites_Path}images"),
                TeensyFileType.Art => new DirectoryPath($"{Favorites_Path}images"),
                TeensyFileType.Aas => new DirectoryPath($"{Favorites_Path}images"),
                TeensyFileType.Hpi => new DirectoryPath($"{Favorites_Path}images"),
                TeensyFileType.Hex => new DirectoryPath("/firmware"),

                _ => new DirectoryPath($"{Favorites_Path}unknown")
            };
        }
    }
}