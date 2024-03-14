using Newtonsoft.Json;
using System.Runtime;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Settings
{
    public enum TeensyLibraryType
    {
        All,
        Programs,
        Music,
        Hex
    }
    public class TeensyLibrary
    {
        public TeensyLibraryType Type { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
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
        public List<TeensyLibrary> Libraries { get; set; } = [];
        public string AutoTransferPath { get; set; } = "auto-transfer";
        public bool AutoFileCopyEnabled { get; set; }
        public bool SaveMusicCacheEnabled { get; set; } = true;        
        public bool FirstTimeSetup { get; set; } = true;

        public TeensySettings()
        {
            GetDefaultBrowserDownloadPath();
        }

        public void InitializeDefaults()
        {
            Libraries.AddRange(new List<TeensyLibrary>
            {
                new TeensyLibrary
                {
                    Type = TeensyLibraryType.All,
                    DisplayName = "All",
                    Icon = "AllInclusive",
                    Path = "/",
                    IsConfigurable = false

                },
                new TeensyLibrary
                {
                    Type = TeensyLibraryType.Music,
                    DisplayName = "Music",
                    Icon = "MusicClefTreble",
                    Path = "libraries/music"
                },
                new TeensyLibrary
                {
                    Type = TeensyLibraryType.Programs,
                    DisplayName = "Games",
                    Icon = "Ghost",
                    Path = "libraries/programs"
                },
                new TeensyLibrary
                {
                    Type = TeensyLibraryType.Hex,
                    DisplayName = "Hex",
                    Path = "libraries/hex"
                }
            });
            FileTargets.AddRange(new List<TeensyTarget>
            {
                new TeensyTarget
                {
                    Type = TeensyFileType.Sid,
                    LibraryType = TeensyLibraryType.Music,
                    DisplayName = "SID",
                    Extension = ".sid"
                },
                new TeensyTarget
                {
                    Type = TeensyFileType.Prg,
                    LibraryType = TeensyLibraryType.Programs,
                    DisplayName = "PRG",
                    Extension = ".prg"
                },
                new TeensyTarget
                {
                    Type = TeensyFileType.Crt,
                    LibraryType = TeensyLibraryType.Programs,
                    DisplayName = "CRT",
                    Extension = ".crt"
                },
                new TeensyTarget
                {
                    Type = TeensyFileType.Hex,
                    LibraryType = TeensyLibraryType.Hex,
                    DisplayName = "HEX",
                    Extension = ".hex"
                }
            });
        }

        public string GetFileTypePath(TeensyFileType type)
        {
            var target = FileTargets.FirstOrDefault(t => t.Type == type) 
                ?? throw new TeensyException($"Unsupported file type: {type}");

            var library = Libraries.FirstOrDefault(l => l.Type == target.LibraryType) 
                ?? throw new TeensyException($"Unsupported library type: {target.LibraryType}");

            return library.Path;
        }

        public string GetLibraryPath(TeensyLibraryType type)
        {
            var library = Libraries.FirstOrDefault(l => l.Type == type) 
                ?? throw new TeensyException($"Unsupported library type: {type}");

            return library.Path;
        }

        public List<string> GetFavoritePaths() => FileTargets.Select(t => GetFavoritePath(t.Type)).ToList();

        public string GetFavoritePath(TeensyFileType type) 
        {
            return type switch
            {
                TeensyFileType.Sid => GetFileTypePath(TeensyFileType.Sid)
                    .UnixPathCombine("/playlists/favorites"),

                TeensyFileType.Prg => GetFileTypePath(TeensyFileType.Prg)
                    .UnixPathCombine("/favorites"),

                TeensyFileType.Crt => GetFileTypePath(TeensyFileType.Crt)
                    .UnixPathCombine("/favorites"),

                TeensyFileType.Hex => GetFileTypePath(TeensyFileType.Hex)
                    .UnixPathCombine("/favorites"),

                _ => throw new TeensyException("This file type is not supported for favoriting")
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