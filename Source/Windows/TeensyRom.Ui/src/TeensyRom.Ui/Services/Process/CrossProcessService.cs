using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using System.Reactive.Linq;
using System.Linq;
using TeensyRom.Core.Common;

namespace TeensyRom.Ui.Services.Process
{

    public interface ICrossProcessService
    {
        Task LaunchFile(ILaunchableItem file);
        Task UpsertFile(IFileItem file);
        Task ReorderFiles(IEnumerable<IFileItem> files);
        Task CopyFiles(IEnumerable<CopyFileItem> files);
        Task SaveFavorite(ILaunchableItem file);
        Task RemoveFavorite(ILaunchableItem file);
        Task SendMessageAsync<T>(ProcessCommand<T> message);
    }

    public class CrossProcessService : ICrossProcessService, IDisposable
    {
        private readonly ILaunchFileProcess _launchFile;
        private readonly IUpsertFileProcess _upsertFile;
        private readonly ICopyFileProcess _copyFile;
        private readonly IFavoriteFileProcess _favFile;
        private readonly ILoggingService _log;
        private readonly ISettingsService _settingsService;
        private readonly ISyncService _syncService;
        private readonly INamedPipeServer _server;
        private readonly INamedPipeClient _client;
        private TeensySettings _settings = new();
        private IDisposable? _settingsSubscription;
        private CancellationTokenSource? _listenerCts;

        public CrossProcessService(
            ILaunchFileProcess launchFile,
            IUpsertFileProcess upsertFile,
            ICopyFileProcess copyFile,
            IFavoriteFileProcess favFile,
            ILoggingService log,
            ISettingsService settingsService,
            ISyncService syncService,
            INamedPipeServer server,
            INamedPipeClient client)
        {
            _launchFile = launchFile;
            _upsertFile = upsertFile;
            _copyFile = copyFile;
            _favFile = favFile;
            _log = log;
            _settingsService = settingsService;
            _syncService = syncService;
            _server = server;
            _client = client;
            _settingsSubscription = _settingsService.Settings
                .Do(s => _settings = s)
                .Where(s => s?.LastCart is not null)
                .DistinctUntilChanged(s => (s.SyncFilesEnabled, s.LastCart!.DeviceHash))
                .Subscribe(settings =>
                {
                    if (settings.SyncFilesEnabled)
                    { 
                        _syncService.ProcessCommands(settings.LastCart!);
                    }

                    RestartListener(settings.LastCart!.DeviceHash);
                });
        }

        private void RestartListener(string deviceHash)
        {
            try
            {
                _listenerCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // If already cancelled — safe to ignore
            }
            _listenerCts?.Dispose();

            if (!_settings.SyncFilesEnabled)
            {
                _log.InternalSuccess("Sync service stopped.");
                return;
            }

            _listenerCts = new CancellationTokenSource();
            var ct = _listenerCts.Token;

            var pipeName = $"{PipeConstants.PipePrefix}{deviceHash}";

            _ = Task.Run(() => 
            {
                _server.ListenAsync(pipeName, HandleIncomingMessage, ct);
            }, ct);

            _log.InternalSuccess("Sync service started.");
        }

        private async Task HandleIncomingMessage(ProcessCommandType type, string raw)
        {
            try
            {
                switch (type)
                {
                    case ProcessCommandType.LaunchFile:
                        HandleLaunchFile(raw);
                        break;
                    case ProcessCommandType.CopyFile:
                        await HandleCopyFile(raw);
                        break;

                    case ProcessCommandType.FavoriteFile:
                        await HandleFavoriteFile(raw);
                        break;

                    case ProcessCommandType.RemoveFavoriteFile:
                        await HandleRemoveFavorite(raw);
                        break;

                    case ProcessCommandType.UpsertFile:
                        await HandleUpsertFile(raw);
                        break;

                    case ProcessCommandType.ReorderFiles:
                        await HandleReorderFiles(raw);
                        break;

                    default:
                        _log.InternalError($"Unhandled message type: {type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _log.InternalError($"Error while handling incoming pipe message: {ex.Message}\nRaw: {raw}");
            }
        }

        private void HandleLaunchFile(string raw)
        {
            var message = LaunchableItemSerializer.Deserialize<ProcessCommand<ILaunchableItem>>(raw);

            if (message is null) return;

            _log.InternalSuccess($"Sync Service: Received Launch File Request: {message.Value.Name}");

            _launchFile.LaunchFile(message.Value);
        }

        private async Task HandleReorderFiles(string raw)
        {
            var message = LaunchableItemSerializer.Deserialize<ProcessCommand<List<IFileItem>>>(raw);

            if (message is null) return;

            _log.InternalSuccess($"Sync Service: Received playlist reorder: { message.Value.First().Path.GetUnixParentPath() }");
            await _upsertFile.ReorderFiles(message.Value);
        }

        private async Task HandleUpsertFile(string raw) 
        {
            var message = LaunchableItemSerializer.Deserialize<ProcessCommand<ILaunchableItem>>(raw);

            if (message is null) return;

            _log.InternalSuccess($"Sync Service: Received Upsert File Request: {message.Value.Name}");
            await _upsertFile.UpsertFile(message.Value);
        }

        private async Task HandleCopyFile(string raw)
        {
            var message = LaunchableItemSerializer.Deserialize<ProcessCommand<List<CopyFileItem>>>(raw);

            if (message?.Value is { Count: > 0 })
            {
                _log.InternalSuccess($"Sync Service: Received Copy File Request: {message.Value.First().SourceItem.Name}");
                await _copyFile.CopyFiles(message.Value);
            }
        }

        private async Task HandleFavoriteFile(string raw)
        {
            var message = LaunchableItemSerializer.Deserialize<ProcessCommand<ILaunchableItem>>(raw);
            if (message?.Value != null)
            {
                _log.InternalSuccess($"Sync Service: Received Favorite Request: {message.Value.Name}");
                await _favFile.SaveFavorite(message.Value);
            }
        }

        private async Task HandleRemoveFavorite(string raw)
        {
            var message = LaunchableItemSerializer.Deserialize<ProcessCommand<ILaunchableItem>>(raw);

            if (message?.Value != null)
            {
                _log.InternalSuccess($"Sync Service: Received Removed Favorite Request: {message.Value.Name}");
                await _favFile.RemoveFavorite(message.Value);
            }
        }

        public async Task LaunchFile(ILaunchableItem file)
        {
            await SendMessageAsync(new ProcessCommand<ILaunchableItem>
            {
                MessageType = ProcessCommandType.LaunchFile,
                Value = file
            });
        }
        public async Task ReorderFiles(IEnumerable<IFileItem> files)
        {
            await _upsertFile.ReorderFiles(files);

            await SendMessageAsync(new ProcessCommand<IEnumerable<IFileItem>>
            {
                MessageType = ProcessCommandType.ReorderFiles,
                Value = files
            });
        }

        public async Task UpsertFile(IFileItem file)
        {
            await _upsertFile.UpsertFile(file);

            await SendMessageAsync(new ProcessCommand<IFileItem>
            {
                MessageType = ProcessCommandType.UpsertFile,
                Value = file
            });
        }

        public async Task CopyFiles(IEnumerable<CopyFileItem> files)
        {
            var list = files.ToList();
            await _copyFile.CopyFiles(list);

            await SendMessageAsync(new ProcessCommand<List<CopyFileItem>>
            {
                MessageType = ProcessCommandType.CopyFile,
                Value = list
            });
        }

        public async Task SaveFavorite(ILaunchableItem file)
        {            
            await SendMessageAsync(new ProcessCommand<ILaunchableItem>
            {
                MessageType = ProcessCommandType.FavoriteFile,
                Value = file
            });
            await _favFile.SaveFavorite(file);
        }

        public async Task RemoveFavorite(ILaunchableItem file)
        {            
            await SendMessageAsync(new ProcessCommand<ILaunchableItem>
            {
                MessageType = ProcessCommandType.RemoveFavoriteFile,
                Value = file
            });
            await _favFile.RemoveFavorite(file);
        }

        public async Task SendMessageAsync<T>(ProcessCommand<T> message)
        {
            if (!_settings.SyncFilesEnabled) return;

            var myHash = _settingsService.GetSettings().LastCart?.DeviceHash;

            if (string.IsNullOrEmpty(myHash)) return;

            var knownCarts = _settingsService
                .GetSettings().KnownCarts
                .Where(c => c.DeviceHash != myHash);

            foreach (var cart in knownCarts)
            {
                var pipeName = $"{PipeConstants.PipePrefix}{cart.DeviceHash}";

                var json = LaunchableItemSerializer.Serialize(message);

                try
                {
                    var sent = await _client.SendAsync(pipeName, json);

                    if (!sent)
                    {
                        switch (message.MessageType)
                        {
                            case ProcessCommandType.LaunchFile:

                                _log.InternalError($"Sync Failed (Launch): Cart {cart.Name} was not found.");
                                break;

                            case ProcessCommandType.UpsertFile:

                                _log.InternalError($"Sync Failed (Upsert): Cart {cart.Name} was not found.  Queuing for later.");

                                if (message.Value is IFileItem item)
                                {
                                    _syncService.QueueUpsertFiles(cart, [item]);
                                }
                                break;

                            case ProcessCommandType.ReorderFiles:

                                _log.InternalError($"Sync Failed (Reorder): Cart {cart.Name} was not found.  Queuing for later.");

                                if (message.Value is IEnumerable<IFileItem> reorderItems)
                                {
                                    _syncService.QueueReorderFiles(cart, reorderItems.ToList());
                                }
                                break;

                            case ProcessCommandType.CopyFile:

                                _log.InternalError($"Sync Failed (Favorite): Cart {cart.Name} was not found.  Queuing for later.");

                                if (message.Value is List<CopyFileItem> items) 
                                {
                                    _syncService.QueueCopyFiles(cart, items);
                                }   
                                break;

                            case ProcessCommandType.FavoriteFile:

                                _log.InternalError($"Sync Failed (Playlist): Cart {cart.Name} was not found.  Queuing for later.");

                                if (message.Value is ILaunchableItem favItem)
                                {
                                    _syncService.QueueFavFile(cart, favItem);
                                }                                    
                                break;

                            case ProcessCommandType.RemoveFavoriteFile:
                                
                                _log.InternalError($"Sync Failed (Remove Playlist): Cart {cart.Name} was not found.  Queuing for later.");
                                
                                if (message.Value is ILaunchableItem removeFavItem)
                                {
                                    _syncService.QueueRemoveFavFile(cart, removeFavItem);
                                }                                    
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.InternalError($"Sync Failed: Error sending to Cart {cart.Name}:");
                    _log.InternalError(ex.Message);
                }
            }
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
            _listenerCts?.Cancel();
            _listenerCts?.Dispose();
        }
    }
}