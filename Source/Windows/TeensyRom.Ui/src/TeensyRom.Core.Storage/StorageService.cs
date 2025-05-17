using MediatR;
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
        public List<string> BannedDirectories { get; set; } = [];
        public List<string> BannedFiles { get; set; } = [];
    }
    public class StorageService(IStorageCache cache, StorageSettings settings, IMediator mediator, IAlertService alert, ILoggingService log, ISidMetadataService sidMetadata, IGameMetadataService gameMetadata) : IStorageService
    {
        public async Task<IFileItem?> GetFile(string path)
        {
            var parentPath = path.GetUnixParentPath();

            var directory = await GetDirectory(parentPath);

            if (directory is null) 
            {
                log.InternalWarning($"The parent directory {parentPath} was not found");
                return null;
            }
            var file = directory.Files
                .FirstOrDefault(f => f.Path.RemoveLeadingAndTrailingSlash()
                    .Equals(path.RemoveLeadingAndTrailingSlash(), StringComparison.OrdinalIgnoreCase));

            if (file is null) 
            {
                log.InternalWarning($"The file {path.GetFileNameFromPath()} was not found in the directory {parentPath}");
                return null;
            }
            return file;
        }

        public async Task<IStorageCacheItem?> GetDirectory(string path)
        {
            var cacheItem = cache.GetByDirPath(path);

            if (cacheItem != null)
            {
                return cacheItem;
            }

            var response = await mediator.Send(new GetDirectoryCommand(settings.CartStorage.Type, path, settings.CartStorage.DeviceId));

            if (!response.IsSuccess) 
            {
                return null;
            }
            var filteredContent = FilterBannedItems(response.DirectoryContent);

            if (filteredContent is null) return null;

            cacheItem = await SaveDirectoryToCache(filteredContent);

            cache.WriteToDisk();
            return cacheItem;
        }        
        public async Task<bool> CacheAll()
        {
            return await Cache(StorageHelper.Remote_Path_Root);
        }
        public async Task<bool> Cache(string path)
        {
            if (path == StorageHelper.Remote_Path_Root)
            {
                ClearCache();
            }
            else
            {
                ClearCache(path);
            }
            alert.Publish("Resetting TeensyROM");

            var _ = await mediator.Send(new ResetCommand
            {
                DeviceId = settings.CartStorage.DeviceId
            });
            alert.Publish($"Refreshing cache for {path} and all nested directories.");
            var response = await mediator.Send(new GetDirectoryRecursiveCommand(settings.CartStorage.Type, path, settings.CartStorage.DeviceId));

            if (!response.IsSuccess) return false;

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
                cache.EnsureFavorites([.. StorageHelper.FavoritePaths]);
                cache.WriteToDisk();
            });
            alert.Publish($"Indexing completed for {settings.CartStorage.Type} storage.");

            return true;
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

        private async Task<IStorageCacheItem> SaveDirectoryToCache(DirectoryContent dirContent)
        {
            IStorageCacheItem? cacheItem;
            var files = MapAndOrderFiles(dirContent);

            var playlistFile = dirContent.Files
                        .FirstOrDefault(f => f.Path.GetFileNameFromPath() == StorageHelper.Playlist_File_Name);

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

            var favPaths = StorageHelper.FavoritePaths;

            if (favPaths.Any(dirContent.Path.Contains)) FavCacheItems(cacheItem);

            cache.UpsertDirectory(dirContent.Path, cacheItem);
            return cacheItem;
        }

        private static void FavCacheItems(IStorageCacheItem cacheItem) => cacheItem.Files.ForEach(f => f.IsFavorite = true);

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
