using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;

namespace TeensyRom.Ui.Services.Process
{
    public interface ISyncService
    {
        void QueueReorderFiles(KnownCart cart, List<IFileItem> items);
        void QueueUpsertFiles(KnownCart cart, List<IFileItem> items);
        void QueueCopyFiles(KnownCart cart, List<CopyFileItem> items);
        void QueueFavFile(KnownCart cart, ILaunchableItem command);
        Task ProcessCommands(KnownCart cart);
        void QueueRemoveFavFile(KnownCart cart, ILaunchableItem file);
    }

    public class SyncService(ICopyFileProcess fileCopyProcess, IFavoriteFileProcess favoriteFileProcess, IUpsertFileProcess upsertFile, IAlertService alert, ILoggingService logger) : ISyncService
    {
        private const string SyncReorderFilesPrefix = "SyncReorderFiles";
        private const string SyncUpsertFilesPrefix = "SyncUpsertFiles";
        private const string SyncFavoritesPrefix = "SyncFavoriteFiles";
        private const string SyncRemoveFavoritesPrefix = "SyncRemoveFavoriteFiles";
        private const string SyncCopyPrefix = "SyncCopyFiles";

        public void QueueReorderFiles(KnownCart cart, List<IFileItem> files)
        {
            var path = GetSyncFilePath(SyncReorderFilesPrefix, cart.DeviceHash);
            AddToSyncFile(path, files);
            logger.Internal($"Queued file reorders for next sync");
        }

        public void QueueUpsertFiles(KnownCart cart, List<IFileItem> files)
        {
            var path = GetSyncFilePath(SyncUpsertFilesPrefix, cart.DeviceHash);
            AddToSyncFile(path, files);
            logger.Internal($"Queued file updates for next sync");
        }

        public void QueueFavFile(KnownCart cart, ILaunchableItem file)
        {
            var path = GetSyncFilePath(SyncFavoritesPrefix, cart.DeviceHash);
            AddToSyncFile(path, [file]);
            logger.Internal($"Queued favorite item for next sync: {file.Name}");            
        }

        public void QueueRemoveFavFile(KnownCart cart, ILaunchableItem file)
        {
            var path = GetSyncFilePath(SyncRemoveFavoritesPrefix, cart.DeviceHash);
            AddToSyncFile(path, [file]);
            logger.Internal($"Queued favorite item for next sync: {file.Name}");
        }

        public void QueueCopyFiles(KnownCart cart, List<CopyFileItem> itemsForQueue)
        {
            var filePath = GetSyncFilePath(SyncCopyPrefix, cart.DeviceHash);
            AddToSyncFile(filePath, itemsForQueue);
            itemsForQueue.ForEach(item => logger.Internal($"Queued playlist item for next sync: {item.TargetPath}"));
        }

        public async Task ProcessCommands(KnownCart cart)
        {
            await Task.Delay(200); //TODO: I noticed a race condition on removing favorites while cache was being saved to disk.  Ugly workaround, I know.

            await ProcessSyncFile<ILaunchableItem>(
                GetSyncFilePath(SyncFavoritesPrefix, cart.DeviceHash),
                "Syncing favorites queue...",
                async items =>
                {
                    foreach (var item in items)
                    { 
                        await favoriteFileProcess.SaveFavorite(item);
                    }
                });

            await ProcessSyncFile<ILaunchableItem>(
                GetSyncFilePath(SyncRemoveFavoritesPrefix, cart.DeviceHash),
                "Syncing favorite removal queue...",
                async items =>
                {
                    foreach (var item in items)
                    {
                        await favoriteFileProcess.RemoveFavorite(item);
                    }
                });

            await ProcessSyncFile<CopyFileItem>(
                GetSyncFilePath(SyncCopyPrefix, cart.DeviceHash),
                "Syncing playlist queue...",
                fileCopyProcess.CopyFiles);

            await ProcessSyncFile<IFileItem>(
                GetSyncFilePath(SyncUpsertFilesPrefix, cart.DeviceHash),
                "Syncing file updates queue...",
                async items =>
                {
                    foreach (var item in items)
                    {
                        await upsertFile.UpsertFile(item);
                    }
                });

            await ProcessSyncFile<IFileItem>(
                GetSyncFilePath(SyncReorderFilesPrefix, cart.DeviceHash),
                "Syncing file reorders queue...",
                async items => 
                {
                    await upsertFile.ReorderFiles(items);
                });
        }

        private void AddToSyncFile<T>(string filePath, List<T> newItems)
        {
            lock (this)
            {
                List<T> existingItems = [];

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    existingItems = LaunchableItemSerializer.Deserialize<List<T>>(json) ?? [];
                }
                existingItems.AddRange(newItems);
                File.WriteAllText(filePath, LaunchableItemSerializer.Serialize(existingItems));
            }
        }

        private async Task ProcessSyncFile<T>(string filePath, string alertMessage, Func<List<T>, Task> processAction)
        {
            if (!File.Exists(filePath))
                return;

            alert.Publish(alertMessage);
            logger.Internal(alertMessage);

            var json = await File.ReadAllTextAsync(filePath);
            var items = LaunchableItemSerializer.Deserialize<List<T>>(json) ?? [];

            await processAction(items);
            File.Delete(filePath);
        }

        private static string GetSyncFilePath(string prefix, string deviceHash)
        {
            var fileName = $"{prefix}_{deviceHash}.json";
            return Path.Combine(StorageConstants.CachePath, fileName);
        }
    }
}
