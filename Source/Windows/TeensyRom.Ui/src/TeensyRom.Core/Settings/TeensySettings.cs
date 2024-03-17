using Newtonsoft.Json;
using System.Runtime;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Settings
{
    public enum TeensyFilterType
    {
        All,
        Programs,
        Music,
        Hex
    }
    public class TeensyFilter
    {
        public TeensyFilterType Type { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool IsConfigurable { get; set; } = true;
    }
    /// <summary>
    /// Used to persist and retrieve user preference from disk.  See: Settings.json in the bin folder
    /// </summary>
    public record TeensySettings
    {
        public TeensyStorageType TargetType { get; set; } = TeensyStorageType.SD;
        public string WatchDirectoryLocation { get; set; } = string.Empty;
        public string TargetRootPath { get; set; } = @"/";
        public List<TeensyTarget> FileTargets { get; set; } = [];
        public List<TeensyFilter> FileFilters { get; set; } = [];
        public string AutoTransferPath { get; set; } = "auto-transfer";
        public bool AutoFileCopyEnabled { get; set; }
        public bool SaveMusicCacheEnabled { get; set; } = true;        
        public bool FirstTimeSetup { get; set; } = true;
        public List<string> BannedDirectories = ["MUSICIANS/S/Szachista", "System Volume Information", "FOUND.000", "integration-test-files", "integration-tests"];
        public List<string> BannedFiles = ["Revolutionary_Etude_part_1.sid", "Revolutionary_Etude_part_2.sid", "Super_Trouper.sid"];

        public TeensySettings()
        {
            GetDefaultBrowserDownloadPath();
        }

        public void InitializeDefaults()
        {
            FileFilters.AddRange(new List<TeensyFilter>
            {
                new TeensyFilter
                {
                    Type = TeensyFilterType.All,
                    DisplayName = "All",
                    Icon = "AllInclusive",
                    IsConfigurable = false

                },
                new TeensyFilter
                {
                    Type = TeensyFilterType.Music,
                    DisplayName = "Music",
                    Icon = "MusicClefTreble"
                },
                new TeensyFilter
                {
                    Type = TeensyFilterType.Programs,
                    DisplayName = "Games",
                    Icon = "Ghost"
                },
                new TeensyFilter
                {
                    Type = TeensyFilterType.Hex,
                    DisplayName = "Hex"
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
                    FilterType = TeensyFilterType.Programs,
                    DisplayName = "PRG",
                    Extension = ".prg"
                },
                new TeensyTarget
                {
                    Type = TeensyFileType.Crt,
                    FilterType = TeensyFilterType.Programs,
                    DisplayName = "CRT",
                    Extension = ".crt"
                },
                new TeensyTarget
                {
                    Type = TeensyFileType.Hex,
                    FilterType = TeensyFilterType.Hex,
                    DisplayName = "HEX",
                    Extension = ".hex"
                }
            });
        }

        public string GetFileTypePath(TeensyFileType type)
        {
            return FileTargets
                .FirstOrDefault(FileTargets => FileTargets.Type == type)?.Extension
                .RemoveFirstOccurrence(".")

                ?? throw new TeensyException($"Unsupported file type: {type}");
        }

        public List<string> GetFavoritePaths() => FileTargets.Select(t => GetFavoritePath(t.Type)).ToList();

        public string GetFavoritePath(TeensyFileType type) 
        {
            return type switch
            {
                TeensyFileType.Sid => "/favorites/music",
                TeensyFileType.Prg => "/favorites/games",
                TeensyFileType.Crt => "/favorites/games",
                TeensyFileType.Hex => "/firmware",

                _ => throw new TeensyException("This file type is not supported for favoriting")
            }; ;
        }

        public string GetAutoTransferPath(TeensyFileType type)
        {
            return type switch
            {
                TeensyFileType.Sid => AutoTransferPath.UnixPathCombine("music"),
                TeensyFileType.Prg => AutoTransferPath.UnixPathCombine("games"),
                TeensyFileType.Crt => AutoTransferPath.UnixPathCombine("games"),
                TeensyFileType.Hex => "/firmware",

                _ => throw new TeensyException("This file type is not supported for auto-transfer")
            }; ;
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