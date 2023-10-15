using TeensyRom.Core.Files;

namespace TeensyRom.Core.Settings
{
    /// <summary>
    /// Used to persist and retrieve user preference from disk.  See: Settings.json in the bin folder
    /// </summary>
    public class TeensySettings
    {
        public TeensyStorageType TargetType { get; set; } = TeensyStorageType.SD;
        public string WatchDirectoryLocation { get; set; } = string.Empty;        
        public string SidTargetPath { get; set; } = string.Empty;
        public string PrgTargetPath { get; set; } = string.Empty;
        public string CrtTargetPath { get; set; } = string.Empty;
        public string HexTargetPath { get; set; } = string.Empty;

        public TeensySettings()
        {
            GetDefaultBrowserDownloadPath();
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