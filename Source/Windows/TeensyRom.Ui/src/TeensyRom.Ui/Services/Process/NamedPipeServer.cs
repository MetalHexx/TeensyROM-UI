using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using TeensyRom.Core.Logging;
using System.Text.Json;

namespace TeensyRom.Ui.Services.Process
{
    public interface INamedPipeServer
    {
        Task ListenAsync(string pipeName, Func<ProcessCommandType, string, Task> onMessage, CancellationToken ct);
    }
    public class NamedPipeServer(ILoggingService log) : INamedPipeServer
    {

        public async Task ListenAsync(string pipeName, Func<ProcessCommandType, string, Task> onMessage, CancellationToken ct)
        {
            log.Internal($"NamedPipeServer listening on {pipeName}");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(pipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    await server.WaitForConnectionAsync(ct);

                    using var reader = new StreamReader(server);
                    while (server.IsConnected && !ct.IsCancellationRequested)
                    {
                        var raw = await reader.ReadLineAsync(ct);
                        if (raw is null) break;

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