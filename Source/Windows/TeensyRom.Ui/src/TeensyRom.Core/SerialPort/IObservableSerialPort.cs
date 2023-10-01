using System.Reactive;

namespace TeensyRom.Core.Serial
{
    /// <summary>
    /// Provides an observable interface to a serial port that can be interacted with
    /// </summary>
    public interface IObservableSerialPort
    {
        /// <summary>
        /// Connections can be dropped, thus ports can change, so we want to observe those changes.
        /// </summary>
        IObservable<string[]> Ports { get; }
        IObservable<string> Logs { get; }

        /// <summary>
        /// Sets the port to connect to
        /// </summary>
        void SetPort(string port);

        //TOOD: Add an observable value representing a stream of changes to the status of the serial port connection
        //TODO: Add an observable value representing a stream of data received from the serial port
        //TODO: Add a method to Set the selected port state
        //TODO: Add a method to open the port using the selected port
        //TODO: Add a method to close the port
        //TODO: Add a method to send data
    }
}