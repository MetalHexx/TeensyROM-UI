using System.Collections.Immutable;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Settings
{
    public static class StorageHelper 
    {
        public static readonly ImmutableList<TeensyTarget> FileTargets = 
        [
            new TeensyTarget
            {
                Type = TeensyFileType.Sid,
                FilterType = TeensyFilterType.Music,
                DisplayName = "SID",
                Extension = ".sid"
            },
            new TeensyTarget
            {
                Type = TeensyFileType.Prg,
                FilterType = TeensyFilterType.Games,
                DisplayName = "PRG",
                Extension = ".prg"
            },
            new TeensyTarget
            {
                Type = TeensyFileType.P00,
                FilterType = TeensyFilterType.Games,
                DisplayName = "P00",
                Extension = ".p00"
            },
            new TeensyTarget
            {
                Type = TeensyFileType.Crt,
                FilterType = TeensyFilterType.Games,
                DisplayName = "CRT",
                Extension = ".crt"
            },
            new TeensyTarget
            {
                Type = TeensyFileType.Hex,
                FilterType = TeensyFilterType.Hex,
                DisplayName = "HEX",
                Extension = ".hex"
            },
            new TeensyTarget
            {
                Type = TeensyFileType.Kla,
                FilterType = TeensyFilterType.Images,
                DisplayName = "KLA",
                Extension = ".kla"
            },
            new TeensyTarget
            {
                Type = TeensyFileType.Koa,
                FilterType = TeensyFilterType.Images,
                DisplayName = "KOA",
                Extension = ".koa"
            },
            new TeensyTarget
            {
                Type = TeensyFileType.Art,
                FilterType = TeensyFilterType.Images,
                DisplayName = "ART",
                Extension = ".art"
            },
            new TeensyTarget
            {
                Type = TeensyFileType.Aas,
                FilterType = TeensyFilterType.Images,
                DisplayName = "AAS",
                Extension = ".aas"
            },
            new TeensyTarget
            {
                Type = TeensyFileType.Hpi,
                FilterType = TeensyFilterType.Images,
                DisplayName = "HPI",
                Extension = ".hpi"
            },
            new TeensyTarget
            {
                Type = TeensyFileType.D64,
                FilterType = TeensyFilterType.Games,
                DisplayName = "D64",
                Extension = ".d64"
            }
        ];
        public static readonly ImmutableList<TeensyFilter>FileFilters = 
        [
            new TeensyFilter
            {
                Type = TeensyFilterType.All,
                DisplayName = "All",
                Icon = "AllInclusive",

            },
            new TeensyFilter
            {
                Type = TeensyFilterType.Music,
                DisplayName = "Music",
                Icon = "MusicClefTreble"
            },
            new TeensyFilter
            {
                Type = TeensyFilterType.Games,
                DisplayName = "Games",
                Icon = "Ghost"
            },
            new TeensyFilter
            {
                Type = TeensyFilterType.Hex,
                DisplayName = "Hex",
                Icon = "ArrowUpBoldHexagonOutline"
            },
            new TeensyFilter
            {
                Type = TeensyFilterType.Images,
                DisplayName = "Images",
                Icon = "FileImageOutline"
            }
        ];

        public static TeensyFileType[] GetFileTypes(TeensyFilterType filterType)
        {
            if (filterType == TeensyFilterType.All)
            {
                return FileTargets
                    .Where(ft => ft.Type != TeensyFileType.Hex)
                    .Select(t => t.Type).ToArray();
            }
            return FileTargets
                .Where(ft => ft.FilterType == filterType)
                .Select(t => t.Type).ToArray();
        }

        public static string GetFavoritePath(TeensyFileType type)
        {
            return type switch
            {
                TeensyFileType.Sid => "/favorites/music",
                TeensyFileType.Prg => "/favorites/games",
                TeensyFileType.P00 => "/favorites/games",
                TeensyFileType.Crt => "/favorites/games",
                TeensyFileType.Kla => "/favorites/images",
                TeensyFileType.Koa => "/favorites/images",
                TeensyFileType.Art => "/favorites/images",
                TeensyFileType.Aas => "/favorites/images",
                TeensyFileType.Hpi => "/favorites/images",
                TeensyFileType.Hex => "/firmware",

                _ => "/favorites/unknown"
            };
        }
    }
}