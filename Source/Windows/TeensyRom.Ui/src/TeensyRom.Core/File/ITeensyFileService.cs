using System.Reactive;

namespace TeensyRom.Core.File
{
    /// <summary>
    /// Manages file transfers to TeensyRom
    /// </summary>
    public interface ITeensyFileService: IDisposable
    {
        /// <summary>
        /// Emits values on activies performed by the file service
        /// </summary>
        IObservable<string> Logs { get; }

        /// <summary>
        /// Saves a file to TeensyRom
        /// </summary>
        /// <param name="fullPath">Full path to file location</param>
        Unit SaveFile(string fullPath);
    }
}
