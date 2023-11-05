using System.Reactive;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Serial.Services
{
    /// <summary>
    /// Provides an interface to a teensy specific serial port operations.
    /// </summary>
    public interface ITeensyObservableSerialPort : IObservableSerialPort, IDisposable
    {
        /// <summary>
        /// Gets a directory listing from teensy given a path, storage type, skip and take
        /// </summary>
        DirectoryContent? GetDirectoryContent(string path, TeensyStorageType storageType, uint skip, uint take);

        /// <summary>
        /// Sends ping bytes to teensyrom
        /// </summary>        
        Unit PingDevice();

        /// <summary>
        /// Sends reset bytes to teensyrom
        /// </summary>        
        Unit ResetDevice();

        /// <summary>
        /// Sends a file to the TeensROM
        /// </summary>
        bool SendFile(TeensyFileInfo fileInfo);
    }
}