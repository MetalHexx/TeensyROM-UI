using System.Text.Json.Serialization;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Settings
{
    /// <summary>
    /// Used to persist and retrieve user preference from disk.  See: Settings.json in the bin folder
    /// </summary>
    public record TeensySettings
    {   
        [JsonIgnore]
        public KnownCart? LastCart { get; set; }
        public List<KnownCart> KnownCarts { get; set; } = [];
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;
        public string WatchDirectoryLocation { get; set; } = string.Empty;
        public string TargetRootPath { get; set; } = @"/";

        [JsonIgnore]
        public List<TeensyTarget> FileTargets { get; set; } = [];
        [JsonIgnore]
        public List<TeensyFilter> FileFilters { get; set; } = [];
        public DirectoryPath AutoTransferPath { get; set; } = new DirectoryPath("auto-transfer");
        public bool AutoFileCopyEnabled { get; set; } = false;
        public bool AutoLaunchOnCopyEnabled { get; set; } = true;
        public bool AutoConnectEnabled { get; set; } = true;
        public TeensyFilterType StartupFilter { get; set; } = TeensyFilterType.All;
        public bool StartupLaunchEnabled { get; set; } = true;
        public bool StartupLaunchRandom { get; set; } = false;
        public bool RepeatModeOnStartup { get; set; } = false;
        public bool PlayTimerEnabled { get; set; } = false;  
        public bool NavToDirOnLaunch { get; set; } = true;
        public bool MuteFastForward { get; set; } = false;
        public bool MuteRandomSeek { get; set; } = false;
        public bool FirstTimeSetup { get; set; } = true;
        public bool SyncFilesEnabled { get; set; } = false;

        public List<string> BannedDirectories = [];
        public List<string> BannedFiles = [];
        public SearchWeights SearchWeights { get; set; } = new();
        public List<string> SearchStopWords = [];

        public TeensySettings()
        {
            GetDefaultBrowserDownloadPath();
            FileFilters.AddRange(StorageHelper.Filters);
            FileTargets.AddRange(StorageHelper.FileTargets);
        }

        public void InitializeDefaults()
        {
            BannedDirectories = ["MUSICIANS/S/Szachista", "System Volume Information", "FOUND.000", "integration-test-files", "integration-tests", "AlternativeFormats", "Dumps", "Docs"];
            BannedFiles = ["Revolutionary_Etude_part_1.sid", "Revolutionary_Etude_part_2.sid", "Super_Trouper.sid"];
            SearchStopWords = ["a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "is", "it", "no", "not", "of", "on", "or", "that", "the", "to", "was", "with"];
        }

        public string GetFileTypePath(TeensyFileType type)
        {
            var target = FileTargets.FirstOrDefault(t => t.Type == type);
            return target.Equals(default)
                ? string.Empty
                : target.Extension.RemoveFirstOccurrence(".");
        }

        public TeensyFilter GetStartupFilter() => FileFilters.First(f => f.Type == StartupFilter);

        public List<DirectoryPath> GetAllFavoritePaths()
        {
            return
            [
                new DirectoryPath($"{StorageHelper.Favorites_Path}music"),
                new DirectoryPath($"{StorageHelper.Favorites_Path}games"),
                new DirectoryPath($"{StorageHelper.Favorites_Path}text"),
                new DirectoryPath($"{StorageHelper.Favorites_Path}images")
            ];
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