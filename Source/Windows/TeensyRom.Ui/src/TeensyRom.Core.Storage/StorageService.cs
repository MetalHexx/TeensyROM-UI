using MediatR;
using System.Runtime;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.GetFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Games;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Sid;

namespace TeensyRom.Core.Storage
{
    public class StorageSettings 
    {
        public CartStorage CartStorage { get; init; } = null!;
        public List<string> FavoritePaths { get; set; } = [];
        public List<string> BannedDirectories { get; set; } = [];
        public List<string> BannedFiles { get; set; } = [];
    }
    public class StorageService(IStorageCache cache, StorageSettings settings, IMediator mediator, IAlertService alert, ILoggingService log, ISidMetadataService sidMetadata, IGameMetadataService gameMetadata) : IStorageService
    {
        public async Task CacheAll()
        {
            await Cache(StorageConstants.Remote_Path_Root);
        }
        public async Task Cache(string path)
        {
            if (path == StorageConstants.Remote_Path_Root)
            {
                ClearCache();
            }
            else
            {
                ClearCache(path);
            }
            alert.Publish($"Refreshing cache for {path} and all nested directories.");
            var response = await mediator.Send(new GetDirectoryRecursiveCommand(settings.CartStorage.Type, path, settings.CartStorage.DeviceId));

            if (!response.IsSuccess) return;

            alert.Publish($"Enriching music and games.");

            await Task.Run(async () =>
            {
                foreach (var directory in response.DirectoryContent)
                {
                    if (directory is null) continue;

                    var filteredContent = FilterBannedItems(directory);

                    if (filteredContent is null) continue;

                    await SaveDirectoryToCache(filteredContent);
                }
                cache.EnsureFavorites(settings.FavoritePaths);
                cache.WriteToDisk();
            });
            alert.Publish($"Indexing completed for {settings.CartStorage.Type} storage.");
        }

        public void ClearCache() => cache.ClearCache();
        public void ClearCache(string path) => cache.DeleteDirectoryWithChildren(path);

        private DirectoryContent? FilterBannedItems(DirectoryContent directoryContent)
        {
            var pathBanned = settings.BannedDirectories.Any(d => directoryContent.Path.Contains(d));
            if (pathBanned) return null;

            var filteredDirectories = directoryContent.Directories.Where(d => settings.BannedDirectories.All(b => !d.Path.Contains(b)));
            var filteredFiles = directoryContent.Files.Where(f => settings.BannedFiles.All(b => !f.Name.Contains(b)));

            return new DirectoryContent
            {
                Path = directoryContent.Path,
                Directories = filteredDirectories.ToList(),
                Files = filteredFiles.ToList()
            };
        }

        private async Task<StorageCacheItem> SaveDirectoryToCache(DirectoryContent dirContent)
        {
            StorageCacheItem? cacheItem;
            var files = MapAndOrderFiles(dirContent);

            var playlistFile = dirContent.Files
                        .FirstOrDefault(f => f.Path.GetFileNameFromPath() == StorageConstants.Playlist_File_Name);

            if (playlistFile is not null)
            {
                var customResult = await mediator.Send(new GetFileCommand(settings.CartStorage.Type, playlistFile.Path, settings.CartStorage.DeviceId));

                var playlist = LaunchableItemSerializer.Deserialize<Playlist>(customResult.FileData);

                if (playlist is not null)
                {
                    foreach (var file in files)
                    {
                        var customization = playlist.Items
                            .FirstOrDefault(c => c?.FilePath.RemoveLeadingAndTrailingSlash() == file.Path.RemoveLeadingAndTrailingSlash());

                        file.Custom = customization;
                    }
                }
            }


            cacheItem = new StorageCacheItem
            {
                Path = dirContent.Path,
                Directories = dirContent.MapAndOrderDirectories(),
                Files = files
            };

            var favPaths = settings.FavoritePaths;

            if (favPaths.Any(dirContent.Path.Contains)) FavCacheItems(cacheItem);

            cache.UpsertDirectory(dirContent.Path, cacheItem);
            return cacheItem;
        }

        private static void FavCacheItems(StorageCacheItem cacheItem) => cacheItem.Files.ForEach(f => f.IsFavorite = true);

        private List<IFileItem> MapAndOrderFiles(DirectoryContent? directoryContent)
        {
            return directoryContent?.Files
                .Select(MapFile)
                .OrderBy(file => file.Name)
                .ToList() ?? [];
        }

        public IFileItem MapFile(IFileItem file)
        {
            var mappedFile = file.MapFileItem();
            return mappedFile switch
            {
                SongItem s => sidMetadata.EnrichSong(s),
                GameItem g => gameMetadata.EnrichGame(g),
                HexItem h => HexItem.MapHexItem(h),
                ImageItem i => i,
                _ => file
            };
        }
    }
}
