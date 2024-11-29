using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Settings
{
    /// <summary>
    /// Used to persist and retrieve user preference from disk.  See: json in the bin folder
    /// </summary>
    public record TeensySettings
    {
        public TeensyStorageType StorageType { get; set; } = TeensyStorageType.SD;
        public bool AlwaysPromptStorage { get; set; } = true;
        public string WatchDirectoryLocation { get; set; } = string.Empty;
        public string TargetRootPath { get; set; } = @"/";
        public List<TeensyTarget> FileTargets { get; set; } = [];
        public List<TeensyFilter> FileFilters { get; set; } = [];
        public string AutoTransferPath { get; set; } = "auto-transfer";
        public bool AutoFileCopyEnabled { get; set; } = false;
        public bool AutoLaunchOnCopyEnabled { get; set; } = true;
        public bool AutoConnectEnabled { get; set; } = true;
        public TeensyFilterType StartupFilter { get; set; } = TeensyFilterType.All;
        public bool StartupLaunchEnabled { get; set; } = true;
        public bool PlayTimerEnabled { get; set; } = false;
        public bool SaveMusicCacheEnabled { get; set; } = true;
        public bool NavToDirOnLaunch { get; set; } = true;
        public bool FirstTimeSetup { get; set; } = true;
        public List<string> BannedDirectories = [];
        public List<string> BannedFiles = [];
        public SearchWeights SearchWeights { get; set; } = new();
        public List<string> SearchStopWords = [];
        public bool EnableDebugLogs { get; set; } = false;
        public bool HasIndexed { get; set; } = false;

        public TeensySettings()
        {
            GetDefaultBrowserDownloadPath();
        }

        public void InitializeDefaults()
        {
            BannedDirectories = ["MUSICIANS/S/Szachista", "System Volume Information", "FOUND.000", "integration-test-files", "integration-tests", "AlternativeFormats", "Dumps", "Docs"];
            BannedFiles = ["Revolutionary_Etude_part_1.sid", "Revolutionary_Etude_part_2.sid", "Super_Trouper.sid"];
            SearchStopWords = ["a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "is", "it", "no", "not", "of", "on", "or", "that", "the", "to", "was", "with"];
            FileTargets = [.. StorageHelper.FileTargets];
            FileFilters = [.. StorageHelper.FileFilters];      
        }

        public string GetFileTypePath(TeensyFileType type)
        {
            return FileTargets
                .FirstOrDefault(FileTargets => FileTargets.Type == type)?.Extension
                .RemoveFirstOccurrence(".")

                ?? throw new TeensyException($"Unsupported file type: {type}");
        }

        public TeensyFilter GetStartupFilter() => FileFilters.First(f => f.Type == StartupFilter);

        public List<string> GetFavoritePaths() => FileTargets.Select(t => StorageHelper.GetFavoritePath(t.Type)).ToList();

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

        public TeensySettings GetClone() => this with
        {
            BannedDirectories = BannedDirectories.Select(d => d).ToList(),
            BannedFiles = BannedFiles.Select(f => f).ToList(),
            SearchStopWords = SearchStopWords.Select(w => w).ToList(),
            FileFilters = FileFilters.Select(f => new TeensyFilter
            {
                DisplayName = f.DisplayName,
                Icon = f.Icon,
                Type = f.Type,

            }).ToList(),
            FileTargets = FileTargets.Select(t => new TeensyTarget
            {
                DisplayName = t.DisplayName,
                Extension = t.Extension,
                FilterType = t.FilterType,
                Type = t.Type
            }).ToList(),
            SearchWeights = new SearchWeights
            {
                Creator = SearchWeights.Creator,
                Description = SearchWeights.Description,
                FileName = SearchWeights.FileName,
                FilePath = SearchWeights.FilePath,
                Title = SearchWeights.Title
            }
        };

    }
}