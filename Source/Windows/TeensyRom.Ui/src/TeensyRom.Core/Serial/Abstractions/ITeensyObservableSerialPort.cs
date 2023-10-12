using System.Reactive;
using TeensyRom.Core.Files;

namespace TeensyRom.Core.Serial.Abstractions
{
    /// <summary>
    /// Provides an interface to a teensy specific serial port operations.
    /// </summary>
    public interface ITeensyObservableSerialPort : IObservableSerialPort, IDisposable
    {
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