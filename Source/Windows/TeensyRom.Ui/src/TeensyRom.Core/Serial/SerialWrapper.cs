using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Serial
{
    public interface ISerialWrapper : IDisposable
    {
        int BaudRate { get; }
        int BytesToRead { get; }
        bool IsOpen { get; }
        string PortName { get; set; }
        int ReadTimeout { get; }
        int WriteTimeout { get; }

        event SerialDataReceivedEventHandler DataReceived;

        void Close();
        void DiscardInBuffer();
        void DiscardOutBuffer();
        void Open();
        int Read(byte[] buffer, int offset, int count);
        int ReadByte();
        void Write(string text);
        void Write(byte[] buffer, int offset, int count);
        void Write(char[] buffer, int offset, int count);
    }

    public class SerialWrapper : ISerialWrapper
    {
        private SerialPort _serialPort = new();

        public int BytesToRead => _serialPort.BytesToRead;

        public string PortName
        {
            get { return _serialPort.PortName; }
            set { _serialPort.PortName = value; }
        }

        public int BaudRate => _serialPort.BaudRate;
        public bool IsOpen => _serialPort.IsOpen;
        public int ReadTimeout => _serialPort.ReadTimeout;
        public int WriteTimeout => _serialPort.WriteTimeout;
        public void Open() => _serialPort.Open();
        public void Close() => _serialPort.Close();
        public void Write(string text) => _serialPort.Write(text);
        public void Write(byte[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);
        public void Write(char[] buffer, int offset, int count) => _serialPort.Write(buffer, offset, count);
        public int Read(byte[] buffer, int offset, int count) => _serialPort.Read(buffer, offset, count);        
        public int ReadByte() => _serialPort.ReadByte();
        public void DiscardInBuffer() => _serialPort.DiscardInBuffer();
        public void DiscardOutBuffer() => _serialPort.DiscardOutBuffer();

        public event SerialDataReceivedEventHandler DataReceived
        {
            add => _serialPort.DataReceived += value;
            remove => _serialPort.DataReceived -= value;
        }

        public void Dispose() => _serialPort.Dispose();
    }
}
