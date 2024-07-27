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
        /// Number of bytes available to read from the serial port
        /// </summary>
        int BytesToRead { get; }

        /// <summary>
        /// Emits serial port state changes 
        /// </summary>
        IObservable<Type> State { get; }

        /// <summary>
        /// Clears the input and output buffers of the serial port
        /// </summary>
        void ClearBuffers();

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
        /// Reads an integer value from the serial port
        /// </summary>
        /// <param name="byteLength">The length of the byte</param>
        /// <returns>The integer value read from the serial port</returns>
        uint ReadIntBytes(short byteLength);

        /// <summary>
        /// Marks the serial port as locked and gives the caller exclusive access to the port.
        /// Auto-polling behavior is disabled while the port is locked.
        /// </summary>
        void Lock();
        
        /// <summary>
        /// Removes the lock on the serial port and automatically reads and logs
        /// the serial port on an interval.
        /// </summary>        
        void Unlock();
        
        /// <summary>
        /// Reads the buffer from the serial port
        /// </summary>
        int Read(byte[] buffer, int offset, int count);
        
        /// <summary>
        /// Reads a byte from the serial port
        /// </summary>        
        int ReadByte();

        /// <summary>
        /// Waits a specified number of milliseconds and then reads the serial port as a string
        /// </summary> 
        string ReadSerialAsString(int msToWait = 0);

        /// <summary>
        /// Waits a specified number of milliseconds and then reads the serial port as a string.
        /// Results also recorded to the output log.
        /// </summary>        
        string ReadAndLogSerialAsString(int msToWait = 0);
        
        /// <summary>
        /// Reads the serial port as a byte array
        /// </summary>
        byte[] ReadSerialBytes();

        /// <summary>
        /// Waits a specified number of milliseconds and then reads the serial port as bytes
        /// </summary>
        byte[] ReadSerialBytes(int msToWait = 0);

        /// <summary>
        /// Waits for a specified number of bytes to be available to read from the serial port
        /// </summary>
        /// <param name="numBytes"></param>
        /// <param name="timeoutMs">Total time to wait before a timeout exception is thrown</param>
        /// cref="TimeoutException">Thrown if the timeout is reached before the specified number of bytes are available</cref>
        void WaitForSerialData(int numBytes, int timeoutMs);

        /// <summary>
        /// Starts polling the serial port for available ports
        /// </summary>
        void StartPortPoll();
        void StartHealthCheck();
        void StopHealthCheck();
        void EnsureConnection();
    }
}