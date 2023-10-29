using System.Reactive;

namespace TeensyRom.Core.Serial.Services
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

        /// <summary>
        /// The current connection state
        /// </summary>
        IObservable<bool> IsConnected { get; }

        /// <summary>
        /// The current retry connection state
        /// </summary>
        IObservable<bool> IsRetryingConnection { get; }

        /// <summary>
        /// Sets the port to connect to
        /// </summary>
        void SetPort(string port);

        /// <summary>
        /// Opens the port with the current set port
        /// </summary>
        Unit OpenPort();

        /// <summary>
        /// Closes the port
        /// </summary>
        Unit ClosePort();

        /// <summary>
        /// Writes an integer value to the serial ports output buffer
        /// </summary>
        /// <param name="intToSend">The integer</param>
        /// <param name="numBytes">The size of the integer in bytes</param>
        void SendIntBytes(uint intToSend, short numBytes);
    }
}