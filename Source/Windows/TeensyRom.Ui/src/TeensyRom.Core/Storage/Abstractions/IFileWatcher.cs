namespace TeensyRom.Core.Storage.Abstractions
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