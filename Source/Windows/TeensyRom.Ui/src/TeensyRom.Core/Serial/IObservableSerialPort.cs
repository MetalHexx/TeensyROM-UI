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
        int BytesToRead { get; }

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
        Unit SetPort(string port);

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
        /// Marks the serial port as locked and gives the caller exclusive access to the port.
        /// Auto-polling behavior is disabled while the port is locked.
        /// </summary>
        void Lock();
        /// <summary>
        /// Removes the lock on the serial port and engages the auto-polling behavior.
        /// </summary>        
        void Unlock();
        int Read(byte[] buffer, int offset, int count);
        int ReadByte();
        string ReadSerialAsString(int msToWait = 0);
        byte[] ReadSerialBytes();
        void WaitForSerialData(int numBytes, int timeoutMs);
    }
}