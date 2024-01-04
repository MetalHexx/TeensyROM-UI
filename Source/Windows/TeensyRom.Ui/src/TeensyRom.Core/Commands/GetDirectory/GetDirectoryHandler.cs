using MediatR;
using Newtonsoft.Json;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class GetDirectoryHandler : IRequestHandler<GetDirectoryCommand, GetDirectoryResult>
    {
        private TeensySettings _settings;
        private readonly IObservableSerialPort _serialPort;
        private readonly ILoggingService _logService;

        public GetDirectoryHandler(IObservableSerialPort serialPort, ISettingsService settings, ILoggingService logService)
        {
            settings.Settings.Take(1).Subscribe(s => _settings = s);
            _serialPort = serialPort;
            _logService = logService;
        }

        //TODO: refactor skip / take mechanism
        public Task<GetDirectoryResult> Handle(GetDirectoryCommand r, CancellationToken x)
        {
            return Task.Run(() =>
            {
                var aggregatedContent = new DirectoryContent();
                uint totalItemsToFetch = r.Take;
                uint itemsFetched = 0;
                uint currentSkip = r.Skip;
                const int chunkSize = 50;

                while (itemsFetched < totalItemsToFetch)
                {
                    var itemsToFetch = Math.Min(chunkSize, totalItemsToFetch - itemsFetched);

                    var content = GetDirectoryContent(r.Path, _settings.TargetType, currentSkip, itemsToFetch);

                    if (content is null)
                    {
                        throw new TeensyException("There was an error. Received a null result from the request");
                    }

                    aggregatedContent.Add(content);
                    itemsFetched += (uint)content.TotalCount;
                    currentSkip += itemsToFetch;

                    if (content.TotalCount < itemsToFetch)
                    {
                        break;
                    }
                }

                return new GetDirectoryResult
                {
                    DirectoryContent = aggregatedContent
                };
            }, x);
        }

        public DirectoryContent? GetDirectoryContent(string path, TeensyStorageType storageType, uint skip, uint take)
        {
            DirectoryContent? directoryContent = null;

            _serialPort.SendIntBytes(TeensyToken.ListDirectory, 2);

            _serialPort.HandleAck();
            _serialPort.SendIntBytes(storageType.GetStorageToken(), 1);
            _serialPort.SendIntBytes(skip, 2);
            _serialPort.SendIntBytes(take, 2);
            _serialPort.Write($"{path}\0");

            if (WaitForDirectoryStartToken() != TeensyToken.StartDirectoryList)
            {
                _serialPort.ReadSerialAsString(msToWait: 100);
                throw new TeensyException("Error waiting for Directory Start Token");
            }
            directoryContent = ReceiveDirectoryContent();

            if (directoryContent is null)
            {
                _serialPort.ReadSerialAsString(msToWait: 100);
                throw new TeensyException("Error waiting for Directory Start Token");
            }
            directoryContent.Path = path;

            return directoryContent;
        }

        public List<byte> GetRawDirectoryData()
        {
            var receivedBytes = new List<byte>();

            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(10);

            while (true)
            {
                if (DateTime.Now - startTime > timeout)
                {
                    throw new TeensyException($"Timeout waiting for expected reply from TeensyROM -- Received Bytes:\r\n{GetLogString(receivedBytes)}");
                }

                if (_serialPort.BytesToRead > 0)
                {
                    var b = (byte)_serialPort.ReadByte();
                    receivedBytes.Add(b);

                    if (receivedBytes.Count >= 2)
                    {
                        var lastToken = (ushort)(receivedBytes[^2] << 8 | receivedBytes[^1]);
                        if (lastToken == TeensyToken.Fail)
                        {
                            throw new TeensyException($"Received fail token while receiving directory content -- Received Bytes:\r\n{GetLogString(receivedBytes)}");
                        }
                        else if (lastToken == TeensyToken.EndDirectoryList)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
            _logService.Log($"Received Directory Content:\r\n{GetLogString(receivedBytes)}");
            return receivedBytes;
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
            return directoryContent;
        }

        public TeensyToken WaitForDirectoryStartToken()
        {
            _serialPort.WaitForSerialData(numBytes: 2, timeoutMs: 500);

            byte[] recBuf = new byte[2];
            _serialPort.Read(recBuf, 0, 2);
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
