using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.GetFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Games;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music;
using TeensyRom.Core.Settings;
using TeensyRom.Core.ValueObjects;

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
        public async Task<FileItem?> GetFile(FilePath path)
        {
            var directory = await GetDirectory(path.Directory);

            if (directory is null)
            {
                log.InternalWarning($"The parent directory {path.Directory} was not found", settings.CartStorage.DeviceId);
                return null;
            }
            var file = directory.Files
                .FirstOrDefault(f => f.Path.Equals(path));

            if (file is null)
            {
                log.InternalWarning($"The file {path.FileName} was not found in the directory {path.Directory}", settings.CartStorage.DeviceId);
                return null;
            }
            return file;
        }

        public async Task<IStorageCacheItem?> GetDirectory(DirectoryPath path)
        {
            var cacheItem = cache.GetByDirPath(path);

            if (cacheItem != null)
            {
                return cacheItem;
            }

            var response = await mediator.Send(new GetDirectoryRecursiveCommand(
                storageType: settings.CartStorage.Type, 
                path: path, 
                recursive: false, 
                deviceId: settings.CartStorage.DeviceId));

            if (!response.IsSuccess) return null;

            var dirContent = response.DirectoryContent.FirstOrDefault();

            if (dirContent is null)
            {
                log.InternalWarning($"The directory {path} was not found", settings.CartStorage.DeviceId);
                return null;
            }

            var filteredContent = FilterBannedItems(dirContent);

            if (filteredContent is null) return null;

            cacheItem = await SaveDirectoryToCache(filteredContent);

            cache.WriteToDisk();
            return cacheItem;
        }
        public async Task<bool> CacheAll(CancellationToken ct)
        {
            return await Cache(new DirectoryPath(StorageHelper.Remote_Path_Root), ct);
        }
        public async Task<bool> Cache(DirectoryPath path, CancellationToken ct)
        {
            if (path.Equals(new DirectoryPath(StorageHelper.Remote_Path_Root)))
            {
                ClearCache();
            }
            else
            {
                ClearCache(path);
            }
            log.Internal("Resetting TeensyROM", settings.CartStorage.DeviceId);

            var _ = await mediator.Send(new ResetCommand
            {
                DeviceId = settings.CartStorage.DeviceId
            });
            log.Internal($"Refreshing cache for {path} and all nested directories.", settings.CartStorage.DeviceId);

            var getDirectoryCommand = new GetDirectoryRecursiveCommand
            (
                storageType: settings.CartStorage.Type,
                path: path,
                recursive: true,
                deviceId: settings.CartStorage.DeviceId
            );

            var response = await mediator.Send(getDirectoryCommand, ct);

            if (!response.IsSuccess) return false;

            log.Internal($"Enriching music and games.", settings.CartStorage.DeviceId);

            await Task.Run(async () =>
            {
                foreach (var directory in response.DirectoryContent)
                {
                    if (directory is null) continue;

                    var filteredContent = FilterBannedItems(directory);

                    if (filteredContent is null) continue;

                    await SaveDirectoryToCache(filteredContent);
                }
                cache.EnsureFavorites();
                cache.EnsurePlaylists();
                cache.WriteToDisk();
            });
            log.Internal($"Indexing completed for {settings.CartStorage.Type} storage.", settings.CartStorage.DeviceId);

            return true;
        }

        public void ClearCache() => cache.ClearCache();
        public void ClearCache(DirectoryPath path) => cache.DeleteDirectoryWithChildren(path);

        private DirectoryContent? FilterBannedItems(DirectoryContent directoryContent)
        {
            var pathBanned = settings.BannedDirectories.Any(d => directoryContent.Path.Value.Contains(d));
            if (pathBanned) return null;

            var filteredDirectories = directoryContent.Directories.Where(d => settings.BannedDirectories.All(b => !d.Path.ToString().Contains(b)));
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
                        .FirstOrDefault(f => f.Path.FileName == StorageHelper.Playlist_File_Name);

            if (playlistFile is not null)
            {
                var customResult = await mediator.Send(new GetFileCommand(settings.CartStorage.Type, playlistFile.Path, settings.CartStorage.DeviceId));

                var playlist = LaunchableItemSerializer.Deserialize<Playlist>(customResult.FileData);

                if (playlist is not null)
                {
                    foreach (var file in files)
                    {
                        var customization = playlist.Items
                            .FirstOrDefault(c => c?.FilePath.Equals(file.Path) ?? false);

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

        private List<FileItem> MapAndOrderFiles(DirectoryContent? directoryContent)
        {
            return directoryContent?.Files
                .Select(MapFile)
                .OrderBy(file => file.Name)
                .ToList() ?? [];
        }

        public FileItem MapFile(FileItem file)
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

        public LaunchableItem? GetRandomFile(StorageScope scope, DirectoryPath scopePath, TeensyFilterType filterType)
        {
            var fileTypes = GetFileTypes(filterType);
            var excludePaths = GetExcludePaths();
            return cache.GetRandomFile(scope, scopePath, excludePaths, fileTypes);
        }

        public IEnumerable<LaunchableItem> Search(string searchText, TeensyFilterType filterType = TeensyFilterType.All, int skip = 0, int take = 50)
        {
            var fileTypes = GetFileTypes(filterType);
            var excludePaths = GetExcludePaths();
            
            // Use default search weights and stop words from TeensySettings.InitializeDefaults()
            var searchWeights = new SearchWeights();
            var stopSearchWords = new List<string> 
            { 
                "a", "an", "and", "are", "as", "at", "be", "but", "by", "for", 
                "if", "in", "is", "it", "no", "not", "of", "on", "or", "that", 
                "the", "to", "was", "with" 
            };
            
            var allResults = cache.Search(searchText, excludePaths, stopSearchWords, searchWeights, fileTypes);
            return allResults.Skip(skip).Take(take);
        }

        private List<DirectoryPath> GetExcludePaths()
        {
            var excludePaths = StorageHelper.FavoritePaths;
            excludePaths.Add(new DirectoryPath(StorageHelper.Playlist_Path));

            return excludePaths.ToList();
        }
        public TeensyFileType[] GetFileTypes(TeensyFilterType filterType)
        {
            if (filterType == TeensyFilterType.All)
            {
                return StorageHelper.FileTargets
                    .Where(ft => ft.Type != TeensyFileType.Hex)
                    .Select(t => t.Type).ToArray();
            }
            return StorageHelper.FileTargets
                .Where(ft => ft.FilterType == filterType)
                .Select(t => t.Type).ToArray();
        }
    }
}
