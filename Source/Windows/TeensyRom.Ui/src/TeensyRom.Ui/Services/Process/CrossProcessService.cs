using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Services.Process;
using System.Reactive.Linq;
using System.Linq;

public interface ICrossProcessService
{
    Task CopyFiles(IEnumerable<CopyFileItem> files);
    Task SendMessageAsync<T>(ProcessCommand<T> message);
}

public class CrossProcessService : ICrossProcessService, IDisposable
{
    private const string _pipePrefix = "TeensyRomPipe_";

    private readonly ICopyFileProcess _copyFile;
    private readonly ILoggingService _log;
    private readonly ISettingsService _settingsService;

    private IDisposable? _settingsSubscription;
    private CancellationTokenSource? _listenerCts;

    public CrossProcessService(ICopyFileProcess copyFile, ILoggingService log, ISettingsService settingsService)
    {
        _copyFile = copyFile;
        _log = log;
        _settingsService = settingsService;

        _settingsSubscription = _settingsService.Settings
            .Where(s => s is not null && s.LastCart is not null)
            .DistinctUntilChanged(s => s.LastCart!.DeviceHash)
            .Subscribe(settings => RestartListener(settings.LastCart!.DeviceHash));
    }

    private void RestartListener(string deviceHash)
    {
        _listenerCts?.Cancel();
        _listenerCts = new CancellationTokenSource();
        var ct = _listenerCts.Token;

        _ = Task.Run(() => ListenAsync(deviceHash, ct), ct);
    }

    private async Task ListenAsync(string deviceHash, CancellationToken ct)
    {
        string pipeName = $"{_pipePrefix}{deviceHash}";
        _log.Internal($"Starting named pipe listener on {pipeName}");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                await server.WaitForConnectionAsync(ct);

                _log.Internal($"Client connected to {pipeName}");

                using var reader = new StreamReader(server);
                using var writer = new StreamWriter(server) { AutoFlush = true };

                while (server.IsConnected && !ct.IsCancellationRequested)
                {
                    string? raw = null;

                    try
                    {
                        raw = await reader.ReadLineAsync(ct);
                        if (raw is null) break;

                        using var jsonDoc = JsonDocument.Parse(raw);
                        var root = jsonDoc.RootElement;

                        if (!root.TryGetProperty("messageType", out var messageTypeElement))
                            throw new Exception("Missing 'messageType' field.");

                        var type = (ProcessCommandType)messageTypeElement.GetInt32();

                        switch (type)
                        {
                            case ProcessCommandType.CopyFile:
                                var message = LaunchableItemSerializer.Deserialize<ProcessCommand<List<CopyFileItem>>>(raw);
                                if (message?.Value != null)
                                    await _copyFile.CopyFiles(message.Value);
                                break;

                            default:
                                _log.Internal($"Unhandled message type: {type}");
                                break;
                        }

                        await writer.WriteLineAsync("ACK");
                    }
                    catch (IOException ioEx) when (ioEx.Message.Contains("pipe is broken", StringComparison.OrdinalIgnoreCase))
                    {
                        _log.Internal($"Client disconnected on {pipeName}.");
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        _log.Internal($"Read cancelled on {pipeName}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _log.Internal($"Error while handling message: {ex.Message}\nRaw: {raw}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _log.Internal($"Listener on {pipeName} canceled.");
                break;
            }
            catch (Exception ex)
            {
                _log.Internal($"Listener error on {pipeName}: {ex.Message}");
                await Task.Delay(1000, ct);
            }
        }
    }

    public async Task CopyFiles(IEnumerable<CopyFileItem> files)
    {
        var message = new ProcessCommand<List<CopyFileItem>>
        {
            MessageType = ProcessCommandType.CopyFile,
            Value = files.ToList()
        };

        await _copyFile.CopyFiles(files);
        await SendMessageAsync(message);
    }

    public async Task SendMessageAsync<T>(ProcessCommand<T> message)
    {
        var myHash = _settingsService.GetSettings().LastCart?.DeviceHash;
        if (string.IsNullOrEmpty(myHash)) return;

        var knownCarts = _settingsService
            .GetSettings().KnownCarts
            .Where(c => c.DeviceHash != myHash);

        foreach (var cart in knownCarts)
        {
            var targetPipe = $"{_pipePrefix}{cart.DeviceHash}";

            try
            {
                using var client = new NamedPipeClientStream(".", targetPipe, PipeDirection.InOut);

                if (!await TryConnectAsync(client))
                {
                    _log.Internal($"Timed out trying to connect to {cart.Name}");
                    continue;
                }

                using var writer = new StreamWriter(client) { AutoFlush = true };
                using var reader = new StreamReader(client);

                var json = LaunchableItemSerializer.Serialize(message);
                await writer.WriteLineAsync(json);

                var readTask = reader.ReadLineAsync();
                if (await Task.WhenAny(readTask, Task.Delay(500)) == readTask)
                {
                    var response = await readTask;
                    _log.Internal($"Process Reply from {cart.Name}: {response}");
                }
                else
                {
                    _log.Internal($"No response from {cart.Name}");
                }
            }
            catch (Exception ex)
            {
                _log.Internal($"Failed to send to {cart.Name}: {ex.Message}");
            }
        }
    }

    private async Task<bool> TryConnectAsync(NamedPipeClientStream client, int timeoutMs = 500)
    {
        var connectTask = client.ConnectAsync(timeoutMs);
        var completedTask = await Task.WhenAny(connectTask, Task.Delay(timeoutMs));
        return completedTask == connectTask && client.IsConnected;
    }

    public void Dispose()
    {
        _settingsSubscription?.Dispose();
        _listenerCts?.Cancel();
        _listenerCts?.Dispose();
    }
}