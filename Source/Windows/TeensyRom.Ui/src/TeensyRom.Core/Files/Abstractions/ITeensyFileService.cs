using System.Reactive;

namespace TeensyRom.Core.Files.Abstractions
{
    /// <summary>
    /// Manages file transfers to TeensyROM
    /// </summary>
    public interface ITeensyFileService : IDisposable
    {
        /// <summary>
        /// Emits values on activies performed by the file service
        /// </summary>
        IObservable<string> Logs { get; }

        /// <summary>
        /// Saves a file to TeensyROM given a TeensyFileInfo
        /// </summary>
        Unit SaveFile(TeensyFileInfo fileInfo);

        /// <summary>
        /// Saves a file to TeensROM give a path
        /// </summary>
        /// <param name="path">Full path to a file</param>        
        Unit SaveFile(string path);

        /// <summary>
        /// Sets a user defined watch folder
        /// </summary>
        void SetWatchFolder(string fullPath);
    }
}