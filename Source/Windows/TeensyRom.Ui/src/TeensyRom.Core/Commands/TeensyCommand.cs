using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings.Entities;
using TeensyRom.Core.Settings.Services;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public abstract class TeensyCommand: IDisposable
    {
        protected readonly ISettingsService _settingsService;
        protected readonly IObservableSerialPort _serialPort;
        protected readonly ILoggingService _logService;

        private IDisposable _settingsSubscription;
        protected TeensySettings _settings = new();

        public TeensyCommand(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
        {
            _settingsService = settingsService;
            _serialPort = serialPort;
            _logService = logService;
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            _settingsSubscription = _settingsService.Settings
                .Subscribe(settings => _settings = settings);
        }

        protected void WaitForSerialData(int numBytes, int timeoutMs)
        {
            var sw = new Stopwatch();
            sw.Start();

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (_serialPort.BytesToRead >= numBytes) return;
                Thread.Sleep(10);
            }
            throw new TimeoutException("Timed out waiting for data to be received");
        }

        protected bool GetAck()
        {
            WaitForSerialData(numBytes: 2, timeoutMs: 500);

            byte[] recBuf = new byte[2];
            _serialPort.Read(recBuf, 0, 2);
            ushort recU16 = recBuf.ToInt16();

            switch (recU16)
            {
                case TeensyConstants.Ack_Token:
                    _logService.Log("Response: Acknowledgement Token Received");
                    return true;

                case TeensyConstants.Fail_Token:
                    _logService.Log("Response: Acknowledgement Failure Received");
                    return false;

                default:
                    _logService.Log("Response: Unexpected Response that was not an Ack token - " + recBuf[0].ToString("X2") + ":" + recBuf[1].ToString("X2"));
                    return false;
            }
        }

        protected void ReadSerialAsString(int msToWait = 0)
        {
            Thread.Sleep(msToWait);
            if (_serialPort.BytesToRead == 0) return;

            byte[] receivedData = new byte[_serialPort.BytesToRead];
            _serialPort.Read(receivedData, 0, receivedData.Length);

            _logService.Log("Received String: " + Encoding.ASCII.GetString(receivedData));
        }

        public uint GetStorageToken(TeensyStorageType type)
        {
            return type switch
            {
                TeensyStorageType.SD => TeensyConstants.Sd_Card_Token,
                TeensyStorageType.USB => TeensyConstants.Usb_Stick_Token,
                _ => throw new ArgumentException("Unknown Storage Type")
            };
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }
    }
}
