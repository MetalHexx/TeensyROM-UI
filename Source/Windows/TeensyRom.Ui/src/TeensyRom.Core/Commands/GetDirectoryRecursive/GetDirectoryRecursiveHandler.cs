using MediatR;
using Newtonsoft.Json;
using System.IO;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryRecursiveHandler : IRequestHandler<GetDirectoryRecursiveCommand, GetDirectoryRecursiveResult>
    {
        private readonly ISerialStateContext _serialState;
        private readonly ILoggingService _log;

        public GetDirectoryRecursiveHandler(ISerialStateContext serialState, ILoggingService log)
        {
            _serialState = serialState;
            _log = log;
        }

        public Task<GetDirectoryRecursiveResult> Handle(GetDirectoryRecursiveCommand r, CancellationToken x)
        {
            return Task.Run(() =>
            {
                var result = new GetDirectoryRecursiveResult();
                StringBuilder sb = new();
                GetDirectoryContent(r.Path, r.StorageType, result, sb);                
                return result;
            }, x);
        }

        private void GetDirectoryContent(string path, TeensyStorageType storageType, GetDirectoryRecursiveResult result, StringBuilder directoryLogs)
        {            
            _log.Internal($"=> Indexing: {path}");

            DirectoryContent? directoryContent;

            _serialState.SendIntBytes(TeensyToken.ListDirectory, 2);

            _serialState.HandleAck();
            _serialState.SendIntBytes(storageType.GetStorageToken(), 1);
            _serialState.SendIntBytes(0, 2); //skip
            _serialState.SendIntBytes(9999, 2); //take
            _serialState.Write($"{path}\0");

            if (WaitForDirectoryStartToken() != TeensyToken.StartDirectoryList)
            {
                _serialState.ReadAndLogSerialAsString(msToWait: 100);
                throw new TeensyException("Error waiting for Directory Start Token");
            }
            directoryContent = ReceiveDirectoryContent();

            if (directoryContent is null)
            {
                _serialState.ReadAndLogSerialAsString(msToWait: 100);
                throw new TeensyException("Error waiting for Directory Start Token");
            }
            directoryContent.Path = path;

            result.DirectoryContent.Add(directoryContent);                        

            foreach (var directory in directoryContent.Directories)
            {
                GetDirectoryContent(directory.Path, storageType, result, directoryLogs);
            }
        }

        public List<byte> GetRawDirectoryData()
        {
            var receivedBytes = new List<byte>();

            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(300);

            while (true)
            {
                if (DateTime.Now - startTime > timeout)
                {
                    throw new TeensyException($"Timeout waiting for expected reply from TeensyROM -- Received Bytes:\r\n{GetLogString(receivedBytes)}");
                }

                if (_serialState.BytesToRead > 0)
                {
                    byte[] buffer = new byte[_serialState.BytesToRead];
                    int bytesRead = _serialState.Read(buffer, 0, buffer.Length);
                    receivedBytes.AddRange(buffer.Take(bytesRead));

                    ushort lastToken = CheckForLastToken(receivedBytes);
                    if (lastToken == TeensyToken.Fail.Value || lastToken == TeensyToken.EndDirectoryList.Value)
                    {
                        break;
                    }
                }
            }
            return receivedBytes;
        }

        private ushort CheckForLastToken(List<byte> receivedBytes)
        {
            if (receivedBytes.Count >= 2)
            {
                return (ushort)(receivedBytes[^2] << 8 | receivedBytes[^1]);
            }
            return 0;
        }

        private string GetLogString(List<byte> receivedBytes)
        {
            var byteString = string.Empty;
            receivedBytes.ForEach(b => byteString += b.ToString());
            var logString = Encoding.ASCII.GetString(receivedBytes.ToArray(), 0, receivedBytes.Count - 2);
            return logString;
        }

        public DirectoryContent? ReceiveDirectoryContent()
        {
            var receivedBytes = GetRawDirectoryData();
            var directoryContent = new DirectoryContent();

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

                        if (dirItem is null) continue;

                        dirItem.Path = dirItem.Path.Replace("//", "/"); //workaround to save mem on teensy
                        directoryContent.Directories.Add(dirItem);
                        break;

                    case var item when item.StartsWith(fileToken):
                        var fileJson = item.Substring(6);
                        var fileItem = JsonConvert.DeserializeObject<FileItem>(fileJson);

                        if (fileItem is null) continue;

                        fileItem.Path = fileItem.Path.Replace("//", "/"); //workaround to save mem on teensy
                        directoryContent.Files.Add(fileItem);
                        break;
                }
            }
            return directoryContent;
        }

        public TeensyToken WaitForDirectoryStartToken()
        {
            _serialState.WaitForSerialData(numBytes: 2, timeoutMs: 200);

            byte[] recBuf = new byte[2];
            _serialState.Read(recBuf, 0, 2);
            ushort recU16 = recBuf.ToInt16();

            return recU16 switch
            {
                var _ when recU16 == TeensyToken.StartDirectoryList.Value => TeensyToken.StartDirectoryList,
                var _ when recU16 == TeensyToken.Fail.Value => TeensyToken.Fail,
                _ => TeensyToken.Unnknown,
            };
        }
    }
}
