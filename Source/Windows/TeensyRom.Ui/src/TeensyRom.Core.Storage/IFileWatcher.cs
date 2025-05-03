namespace TeensyRom.Core.Storage
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
        IObservable<List<FileInfo>> FilesFound { get; }

        /// <summary>
        /// Enables the watcher with given a path a file filter
        /// </summary>
        /// <param name="fileTypes">For example *.sid</param>
        void Enable(string fullPath, params string[] fileTypes);

        /// <summary>
        /// Disables the file watcher
        /// </summary>
        void Disable();
    }
}