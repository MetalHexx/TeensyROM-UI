using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music.Midi;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Settings
{
    public enum TeensyFilterType
    {
        All,
        Games,
        Music,
        Hex,
        Images
    }
    public class TeensyFilter
    {
        public TeensyFilterType Type { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
    /// <summary>
    /// Used to persist and retrieve user preference from disk.  See: Settings.json in the bin folder
    /// </summary>
    public record TeensySettings
    {
        public KnownCart? LastCart { get; set; }
        public List<KnownCart> KnownCarts { get; set; } = [];
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;
        public string WatchDirectoryLocation { get; set; } = string.Empty;
        public string TargetRootPath { get; set; } = @"/";

        [JsonIgnore]
        public List<TeensyTarget> FileTargets { get; set; } = [];
        [JsonIgnore]
        public List<TeensyFilter> FileFilters { get; set; } = [];
        public string AutoTransferPath { get; set; } = "auto-transfer";
        public bool AutoFileCopyEnabled { get; set; } = false;
        public bool AutoLaunchOnCopyEnabled { get; set; } = true;
        public bool AutoConnectEnabled { get; set; } = true;
        public TeensyFilterType StartupFilter { get; set; } = TeensyFilterType.All;
        public bool StartupLaunchEnabled { get; set; } = true;
        public bool PlayTimerEnabled { get; set; } = false;  
        public bool NavToDirOnLaunch { get; set; } = true;
        public bool MuteFastForward { get; set; } = false;
        public bool MuteRandomSeek { get; set; } = false;
        public bool FirstTimeSetup { get; set; } = true;
        public string? DefaultComPort { get; set; }
        public string? DefaultMinimalComPort { get; set; }
        public MidiSettings MidiSettings { get; set; } = new();

        public List<string> BannedDirectories = [];
        public List<string> BannedFiles = [];
        public SearchWeights SearchWeights { get; set; } = new();
        public List<string> SearchStopWords = [];

        public TeensySettings()
        {
            GetDefaultBrowserDownloadPath();
            FileFilters.AddRange(new List<TeensyFilter>
            {
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
            });
            FileTargets.AddRange(new List<TeensyTarget>
            {
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
                    Type = TeensyFileType.Seq,
                    FilterType = TeensyFilterType.Images,
                    DisplayName = "SEQ",
                    Extension = ".seq"
                },
                new TeensyTarget
                {
                    Type = TeensyFileType.Txt,
                    FilterType = TeensyFilterType.Images,
                    DisplayName = "TXT",
                    Extension = ".txt"
                },
                new TeensyTarget
                {
                    Type = TeensyFileType.D64,
                    FilterType = TeensyFilterType.Games,
                    DisplayName = "D64",
                    Extension = ".d64"
                }
            });
        }

        public void InitializeDefaults()
        {
            BannedDirectories = ["MUSICIANS/S/Szachista", "System Volume Information", "FOUND.000", "integration-test-files", "integration-tests", "AlternativeFormats", "Dumps", "Docs"];
            BannedFiles = ["Revolutionary_Etude_part_1.sid", "Revolutionary_Etude_part_2.sid", "Super_Trouper.sid"];
            SearchStopWords = ["a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "is", "it", "no", "not", "of", "on", "or", "that", "the", "to", "was", "with"];
        }

        public string GetFileTypePath(TeensyFileType type)
        {
            return FileTargets
                .FirstOrDefault(FileTargets => FileTargets.Type == type)?.Extension
                .RemoveFirstOccurrence(".")

                ?? throw new TeensyException($"Unsupported file type: {type}");
        }

        public TeensyFilter GetStartupFilter() => FileFilters.First(f => f.Type == StartupFilter);

        public List<string> GetFavoritePaths() => FileTargets.Select(t => GetFavoritePath(t.Type)).ToList();

        public string GetFavoritePath(TeensyFileType type) 
        {
            return type switch
            {
                TeensyFileType.Sid => "/favorites/music",
                TeensyFileType.Prg => "/favorites/games",
                TeensyFileType.P00 => "/favorites/games",
                TeensyFileType.Crt => "/favorites/games",
                TeensyFileType.Txt => "/favorites/text",
                TeensyFileType.Seq => "/favorites/images",
                TeensyFileType.Kla => "/favorites/images",
                TeensyFileType.Koa => "/favorites/images",
                TeensyFileType.Art => "/favorites/images",
                TeensyFileType.Aas => "/favorites/images",
                TeensyFileType.Hpi => "/favorites/images",
                TeensyFileType.Hex => "/firmware",

                _ => "/favorites/unknown"
            };
        }

        public string GetAutoTransferPath(TeensyFileType type)
        {
            return type switch
            {
                TeensyFileType.Sid => AutoTransferPath.UnixPathCombine("music"),
                TeensyFileType.Prg => AutoTransferPath.UnixPathCombine("games"),
                TeensyFileType.P00 => AutoTransferPath.UnixPathCombine("games"),
                TeensyFileType.Crt => AutoTransferPath.UnixPathCombine("games"),
                TeensyFileType.Kla => AutoTransferPath.UnixPathCombine("images"),
                TeensyFileType.Koa => AutoTransferPath.UnixPathCombine("images"),
                TeensyFileType.Art => AutoTransferPath.UnixPathCombine("images"),
                TeensyFileType.Aas => AutoTransferPath.UnixPathCombine("images"),
                TeensyFileType.Hpi => AutoTransferPath.UnixPathCombine("images"),
                TeensyFileType.Seq => AutoTransferPath.UnixPathCombine("images"),
                TeensyFileType.Txt => AutoTransferPath.UnixPathCombine("text"),
                TeensyFileType.Hex => "/firmware",

                _ => AutoTransferPath.UnixPathCombine("unknown")
            };
        }

        /// <summary>
        /// If the user has no settings file saved yet, we'll default to the 
        /// environmentally defined location for the user profile download directory
        /// </summary>
        private void GetDefaultBrowserDownloadPath()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            WatchDirectoryLocation = Path.Combine(userProfile, "Downloads");
        }
    }
}