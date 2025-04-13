using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using TeensyRom.Core.Logging;
using System.Text.Json;
using System.Text;

namespace TeensyRom.Ui.Services.Process
{
    public interface INamedPipeServer
    {
        Task ListenAsync(string pipeName, Func<ProcessCommandType, string, Task> onMessage, CancellationToken ct);
    }

    public class NamedPipeServer(ILoggingService log) : INamedPipeServer
    {
        private NamedPipeServerStream? _previousServer;

        public async Task ListenAsync(string pipeName, Func<ProcessCommandType, string, Task> onMessage, CancellationToken ct)
        {
            log.Internal($"NamedPipeServer listening on {pipeName}");

            while (!ct.IsCancellationRequested)
            {
                try
                {                    
                    _previousServer?.Dispose();

                    using var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    _previousServer = server;

                    await server.WaitForConnectionAsync(ct);

                    var buffer = new byte[4096];

                    while (server.IsConnected && !ct.IsCancellationRequested)
                    {
                        var messageBuilder = new StringBuilder();

                        do
                        {
                            var bytesRead = await server.ReadAsync(buffer, 0, buffer.Length, ct);
                            if (bytesRead == 0)
                                break;

                            var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            messageBuilder.Append(chunk);

                        } while (!server.IsMessageComplete);

                        var raw = messageBuilder.ToString();
                        if (string.IsNullOrWhiteSpace(raw))
                            continue;

                        var type = GetMessageType(raw);
                        await onMessage(type, raw);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    log.InternalError($"Pipe server error on {pipeName}: {ex.Message}");
                    await Task.Delay(1000, ct);
                }
            }

            _previousServer?.Dispose();
        }

        private static ProcessCommandType GetMessageType(string raw)
        {
            using var doc = JsonDocument.Parse(raw);
            if (!doc.RootElement.TryGetProperty("messageType", out var typeElement))
            {
                throw new Exception("Missing 'messageType' field.");
            }

            return (ProcessCommandType)typeElement.GetInt32();
        }
    }
}
