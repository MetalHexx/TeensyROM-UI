namespace TeensyRom.Core.Files.Abstractions
{
    /// <summary>
    /// Watches a folder and notifies subscribers that a new
    /// file was added to a folder.
    /// </summary>
    public interface IFileWatcher : IDisposable
    {
        /// <summary>
        /// Emits values when files are added
        /// </summary>
        IObservable<FileInfo> FileFound { get; }

        /// <summary>
        /// Configures the watcher given a path a file filer
        /// </summary>
        /// <param name="fileTypes">For example *.sid</param>
        void SetWatchParameters(string fullPath, params string[] fileTypes);
    }
}