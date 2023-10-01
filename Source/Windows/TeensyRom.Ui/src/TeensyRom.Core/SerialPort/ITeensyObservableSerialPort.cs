using System.Reactive;

namespace TeensyRom.Core.Serial
{
    public interface ITeensyObservableSerialPort: IObservableSerialPort, IDisposable
    {
        /// <summary>
        /// Sends ping bytes to teensyrom
        /// </summary>        
        Unit PingDevice();
    }
}