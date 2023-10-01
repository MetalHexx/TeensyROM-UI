using System.Reactive;

namespace TeensyRom.Core.Serial
{
    public interface ITeensyObservableSerialPort: IObservableSerialPort, IDisposable
    {
        /// <summary>
        /// Sends ping bytes to teensyrom
        /// </summary>        
        Unit PingDevice();

        /// <summary>
        /// Sends reset bytes to teensyrom
        /// </summary>        
        Unit ResetDevice();
    }
}