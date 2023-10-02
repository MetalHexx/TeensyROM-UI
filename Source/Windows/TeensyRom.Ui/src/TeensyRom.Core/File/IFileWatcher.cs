namespace TeensyRom.Core.File
{
    /// <summary>
    /// Watches a folder and notifies subscribers that a new
    /// file was added to a folder.
    /// </summary>
    public interface IFileWatcher: IDisposable
    {
        /// <summary>
        /// Emits values when files are added
        /// </summary>
        IObservable<string> FileFound { get; }

        /// <summary>
        /// Configures the watcher given a path a file filer
        /// </summary>
        /// <param name="fileFilter">For example *.sid</param>
        void SetWatchPath(string fullPath, string fileFilter);
    }
}
