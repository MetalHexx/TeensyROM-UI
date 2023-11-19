using System.Reactive;

namespace TeensyRom.Core.Serial
{
    /// <summary>
    /// Provides an observable interface to a serial port that can be interacted with
    /// </summary>
    public interface IObservableSerialPort : IDisposable
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
        /// Checks to see if the serial port is open
        /// </summary>
        bool IsOpen { get; }
        int BytesToRead { get; }
        string[] GetPortNames();

        /// <summary>
        /// Writes text to the serial port
        /// </summary>
        /// <param name="text"></param>
        void Write(string text);

        /// <summary>
        /// Write byte array to serial port
        /// </summary>
        public void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// Write char buffer to serial port
        /// </summary>        
        public void Write(char[] buffer, int offset, int count);

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

        /// <summary>
        /// Disables the polling read to allow you to safely perform more complex
        /// serial communication without interruption.
        /// </summary>
        void DisableAutoReadStream();
        int Read(byte[] buffer, int offset, int count);

        /// <summary>
        /// Begins automatically polling the serial port to read data returned by device
        /// </summary>
        void EnableAutoReadStream();
        int ReadByte();
    }
}