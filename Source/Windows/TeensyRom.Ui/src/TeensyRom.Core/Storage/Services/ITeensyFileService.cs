using System.Reactive;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    /// <summary>
    /// Manages file transfers to TeensyROM
    /// </summary>
    public interface ITeensyFileService : IDisposable
    {
        /// <summary>
        /// Saves a file to TeensyROM given a TeensyFileInfo
        /// </summary>
        Unit SaveFile(TeensyFileInfo fileInfo);

        /// <summary>
        /// Saves a file to TeensROM give a path
        /// </summary>
        /// <param name="path">Full path to a file</param>        
        Unit SaveFile(string path);
    }
}