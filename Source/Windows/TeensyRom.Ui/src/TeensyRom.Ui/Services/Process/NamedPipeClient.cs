using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using TeensyRom.Core.Logging;

namespace TeensyRom.Ui.Services.Process
{
    public interface INamedPipeClient
    {
        Task<bool> SendAsync(string pipeName, string message, CancellationToken ct = default);
    }
    public class NamedPipeClient : INamedPipeClient
    {
        private readonly ILoggingService _log;

        public NamedPipeClient(ILoggingService log)
        {
            _log = log;
        }

        public async Task<bool> SendAsync(string pipeName, string message, CancellationToken ct = default)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
                await client.ConnectAsync(500, ct);

                using var writer = new StreamWriter(client) { AutoFlush = true };
                await writer.WriteLineAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _log.InternalError($"Pipe client failed to connect to {pipeName}: {ex.Message}");
                return false;
            }
        }
    }






}