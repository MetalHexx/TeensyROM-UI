using Newtonsoft.Json;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.Constants;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Extensions;

namespace TeensyRom.Core.Serial.Services
{
    public class TeensyObservableSerialPort : ObservableSerialPort, ITeensyObservableSerialPort
    {
        public TeensyObservableSerialPort(ILoggingService logService): base(logService)
        {

        }

        public Unit PingDevice()
        {
            if (!_serialPort.IsOpen)
            {
                _logService.Log("You must first connect in order to ping the device.");
                return Unit.Default;
            }
            _logService.Log($"Pinging device");

            _serialPort.Write(TeensyConstants.Ping_Bytes.ToArray(), 0, 2);

            return Unit.Default;
        }

        public Unit ResetDevice()
        {
            if (!_serialPort.IsOpen)
            {
                _logService.Log("You must first connect in order to reset the device.");
                return Unit.Default;
            }
            _logService.Log($"Resetting device");

            _serialPort.Write(TeensyConstants.Reset_Bytes.ToArray(), 0, 2);

            return Unit.Default;
        }

        public bool SendFile(TeensyFileInfo fileInfo)
        {
            DisableAutoReadStream();

            _logService.Log($"Sending file transfer token: {TeensyConstants.Send_File_Token}");
            SendIntBytes(TeensyConstants.Send_File_Token, 2);

            WaitForSerialData(numBytes: 2, timeoutMs: 500);

            if (!GetAck())
            {
                ReadSerialAsString();
                return false;
            }

            _logService.Log($"Sending Stream Length: {fileInfo.StreamLength}");
            SendIntBytes(fileInfo.StreamLength, 4);

            _logService.Log($"Sending Checksum: {fileInfo.Checksum}");
            SendIntBytes(fileInfo.Checksum, 2);

            _logService.Log($"Sending SD_nUSB: {TeensyConstants.Sd_Card_Token}");
            SendIntBytes(GetStorageToken(fileInfo.StorageType), 1);

            _logService.Log($"Sending to target path: {fileInfo.TargetPath.UnixPathCombine(fileInfo.Name)}");
            _serialPort.Write($"{fileInfo.TargetPath.UnixPathCombine(fileInfo.Name)}\0");

            if (!GetAck())
            {
                ReadSerialAsString(msToWait: 100);
                return false;
            }
            _logService.Log("File ready for transfer!");

            _logService.Log($"Sending file: {fileInfo.FullPath}");
            var bytesSent = 0;

            while (fileInfo.StreamLength > bytesSent)
            {
                var bytesToSend = 16 * 1024;
                if (fileInfo.StreamLength - bytesSent < bytesToSend) bytesToSend = (int)fileInfo.StreamLength - bytesSent;
                _serialPort.Write(fileInfo.Buffer, bytesSent, bytesToSend);

                _logService.Log("*");
                bytesSent += bytesToSend;
            }

            if (!GetAck())
            {
                ReadSerialAsString(msToWait: 500);
                _logService.Log("File transfer failed.");
                return false;
            }
            _logService.Log("File transfer complete!");

            EnableAutoReadStream();

            return true;
        }

        public DirectoryContent? GetDirectoryContent(string path, TeensyStorageType storageType, uint skip, uint take)
        {
            DisableAutoReadStream();

            _logService.Log($"Sending directory listing token: {TeensyConstants.List_Directory_Token}");
            SendIntBytes(TeensyConstants.List_Directory_Token, 2);

            if (!GetAck())
            {
                ReadSerialAsString();
                return null;
            }

            _logService.Log($"Sending Storage Type: {TeensyConstants.Sd_Card_Token}");
            SendIntBytes(GetStorageToken(storageType), 1);

            _logService.Log($"Sending Skip: {skip}");
            SendIntBytes(skip, 1);

            _logService.Log($"Sending Take: {take}");
            SendIntBytes(take, 1);

            _logService.Log($"Sending path: {path}");
            _serialPort.Write($"{path}\0");

            if (!WaitForDirectoryStartToken())
            {
                ReadSerialAsString(msToWait: 100);
                return null;
            }
            _logService.Log("Ready to receive directory content!");

            var directoryContent = ReceiveDirectoryContent();

            if (directoryContent is null)
            {
                ReadSerialAsString(msToWait: 100);
                _logService.Log("Failed to receive directory content");
                return directoryContent;
            }

            var contentLog = JsonConvert.SerializeObject(directoryContent, new JsonSerializerSettings { Formatting = Formatting.Indented });

            _logService.Log(contentLog);

            EnableAutoReadStream();

            return directoryContent;
        }

        public List<byte> GetRawDirectoryData()
        {
            var receivedBytes = new List<byte>();

            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(10);

            try
            {
                while (true)
                {
                    if (DateTime.Now - startTime > timeout)
                    {
                        _logService.Log("Timeout while receiving directory content");
                        var byteString = string.Empty;
                        receivedBytes.ForEach(b => byteString += b.ToString());
                        throw new TimeoutException("Timeout waiting for expected reply from TeensyROM");
                    }

                    if (_serialPort.BytesToRead > 0)
                    {
                        var b = (byte)_serialPort.ReadByte();
                        receivedBytes.Add(b);

                        if (receivedBytes.Count >= 2)
                        {
                            var lastToken = (ushort)(receivedBytes[^2] << 8 | receivedBytes[^1]);
                            if (lastToken == TeensyConstants.Fail_Token)
                            {
                                _logService.Log("Received fail token while receiving directory content");
                                return receivedBytes;
                            }
                            else if (lastToken == TeensyConstants.End_Directory_List_Token)
                            {
                                _logService.Log("Received End Directory List Token");
                                break;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Log($"Error getting directory content from TeensyROM:");
                _logService.Log($"{ex.Message}");
                throw new TeensyException("Error fetching directory contents from TeensyROM", ex);
            }
            return receivedBytes;
        }

        public DirectoryContent? ReceiveDirectoryContent()
        {
            var receivedBytes = GetRawDirectoryData();
            var directoryContent = new DirectoryContent();

            try
            {
                var data = Encoding.ASCII.GetString(receivedBytes.ToArray(), 0, receivedBytes.Count - 2);

                const string dirToken = "[Dir]";
                const string dirEndToken = "[/Dir]";
                const string fileToken = "[File]";
                const string fileEndToken = "[/File]";

                var directoryChunks = data.Split(new[] { dirEndToken, fileEndToken }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var chunk in directoryChunks)
                {
                    switch (chunk)
                    {
                        case var item when item.StartsWith(dirToken):
                            var dirJson = item.Substring(5);
                            var dirItem = JsonConvert.DeserializeObject<DirectoryItem>(dirJson);
                            dirItem.Path = dirItem.Path.Replace("//", "/"); //workaround to save mem on teensy
                            directoryContent.Directories.Add(dirItem);
                            break;

                        case var item when item.StartsWith(fileToken):
                            var fileJson = item.Substring(6);
                            var fileItem = JsonConvert.DeserializeObject<FileItem>(fileJson);
                            fileItem.Path = fileItem.Path.Replace("//", "/"); //workaround to save mem on teensy
                            directoryContent.Files.Add(fileItem);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Log($"Error parsing directory content from TeensyROM:");
                _logService.Log($"{ex.Message}");
                return null;
            }
            return directoryContent;
        }


        public bool WaitForDirectoryStartToken()
        {
            WaitForSerialData(numBytes: 2, timeoutMs: 500);

            byte[] recBuf = new byte[2];
            _serialPort.Read(recBuf, 0, 2);
            ushort recU16 = ToInt16(recBuf);

            switch (recU16)
            {
                case TeensyConstants.Start_Directory_List_Token:
                    _logService.Log("Response: StartDirectoryToken Received");
                    return true;

                case TeensyConstants.Fail_Token:
                    _logService.Log("Response: Failure Received");
                    return false;

                default:
                    _logService.Log("Response: Unexpected Response that was not an Ack token - " + recBuf[0].ToString("X2") + ":" + recBuf[1].ToString("X2"));
                    return false;
            }
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

        public void ReadSerial()
        {
            if (_serialPort.BytesToRead == 0) return;

            byte[] receivedData = new byte[_serialPort.BytesToRead];
            _serialPort.Read(receivedData, 0, receivedData.Length);

            _logService.Log("Received Bytes: " + BitConverter.ToString(receivedData));
        }

        public void ReadSerialAsString(int msToWait = 0)
        {
            Thread.Sleep(msToWait);
            if (_serialPort.BytesToRead == 0) return;

            byte[] receivedData = new byte[_serialPort.BytesToRead];
            _serialPort.Read(receivedData, 0, receivedData.Length);

            _logService.Log("Received String: " + Encoding.ASCII.GetString(receivedData));
        }

        public bool GetAck()
        {
            WaitForSerialData(numBytes: 2, timeoutMs: 500);

            byte[] recBuf = new byte[2];
            _serialPort.Read(recBuf, 0, 2);
            ushort recU16 = ToInt16(recBuf);

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

        private void WaitForSerialData(int numBytes, int timeoutMs)
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
    }
}