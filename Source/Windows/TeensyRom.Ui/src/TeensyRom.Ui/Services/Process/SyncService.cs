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
        void AddCopyFiles(KnownCart cart, List<CopyFileItem> items);
        void AddFavFile(KnownCart cart, ILaunchableItem command);
        Task ProcessCommands(KnownCart cart);
    }

    public class SyncService(ICopyFileProcess fileCopy, IFavoriteFileProcess favoriteFile, IAlertService alert) : ISyncService
    {
        private const string SyncFavoritesPrefix = "SyncFavoriteFiles";
        private const string SyncCopyPrefix = "SyncCopyFiles";

        public void AddFavFile(KnownCart cart, ILaunchableItem newFile)
        {
            var path = GetSyncFilePath(SyncFavoritesPrefix, cart.DeviceHash);
            AddToSyncFile(path, [newFile]);
        }

        public void AddCopyFiles(KnownCart cart, List<CopyFileItem> newItems)
        {
            var filePath = GetSyncFilePath(SyncCopyPrefix, cart.DeviceHash);
            AddToSyncFile(filePath, newItems);
        }

        public async Task ProcessCommands(KnownCart cart)
        {
            await ProcessSyncFile<ILaunchableItem>(
                GetSyncFilePath(SyncFavoritesPrefix, cart.DeviceHash),
                "Syncing previously tagged favorites...",
                async items =>
                {
                    foreach (var item in items)
                        await favoriteFile.SaveFavorite(item);
                });

            await ProcessSyncFile<CopyFileItem>(
                GetSyncFilePath(SyncCopyPrefix, cart.DeviceHash),
                "Syncing previous playlist updates...",
                fileCopy.CopyFiles);
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
