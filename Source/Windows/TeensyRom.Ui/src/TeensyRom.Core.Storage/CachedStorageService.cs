using MediatR;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.DeleteFile;
using TeensyRom.Core.Commands.GetFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Games;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Settings;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Storage
{
    public class CachedStorageService : ICachedStorageService
    {
        public IObservable<IEnumerable<FileItem>> FilesDeleted => _filesDeleted.AsObservable();
        private Subject<IEnumerable<FileItem>> _filesDeleted = new();
        public IObservable<IEnumerable<FileItem>> FilesAdded => _filesAdded.AsObservable();
        private Subject<IEnumerable<FileItem>> _filesAdded = new();

        public IObservable<IEnumerable<FileItem>> FilesChanged => _filesChanged.AsObservable();
        private Subject<IEnumerable<FileItem>> _filesChanged = new();

        public IObservable<System.Reactive.Unit> StorageReady => _storageCache.StorageReady;

        protected readonly ISettingsService _settingsService;
        protected readonly IAlertService _alert;
        private readonly ILoggingService _log;
        private readonly IGameMetadataService _gameMetadata;
        private readonly ISidMetadataService _sidMetadata;
        private readonly IMediator _mediator;
        private TeensySettings _settings = null!;
        private IDisposable? _settingsSubscription;


        private IStorageCache _storageCache;

        public CachedStorageService(ISettingsService settingsService, IGameMetadataService gameMetadata, ISidMetadataService sidMetadata, IMediator mediator, IAlertService alert, ILoggingService log, IStorageCache storageCache)
        {
            _settingsService = settingsService;
            _gameMetadata = gameMetadata;
            _sidMetadata = sidMetadata;
            _mediator = mediator;
            _alert = alert;
            _log = log;
            _storageCache = storageCache;
            _settings = settingsService.GetSettings();
            _settingsSubscription = _settingsService.Settings
                .Where(s => s is not null && s.LastCart is not null)
                .Subscribe(s => _settings = s);
        }

        public async Task<LaunchableItem?> SaveFavorite(LaunchableItem launchItem)
        {
            var favPath = StorageHelper.GetFavoritePath(launchItem.FileType);
            var newFavPath = favPath.Combine(new FilePath(launchItem.Name));

            var favCommand = new FavoriteFileCommand
            (
                storageType: _settings.StorageType,
                sourcePath: launchItem.Path,
                targetPath: newFavPath
            );

            var result = await _mediator.Send(favCommand);

            if (!result.IsSuccess)
            {
                _alert.Publish($"There was an error tagging {launchItem.Name} as favorite.");
                return null;
            }
            _alert.Publish($"{launchItem.Name} has been tagged as a favorite.");
            _alert.Publish($"A copy was placed in {favPath}.");

            launchItem.IsFavorite = true;
            _storageCache.UpsertFile(launchItem);

            var savedFav = launchItem.Clone();
            savedFav.Path = newFavPath;
            _storageCache.UpsertFile(savedFav);

            var parent = _storageCache.FindParentFile(launchItem);

            if (parent is not null)
            {
                parent.IsFavorite = true;
                _storageCache.UpsertFile(parent);
            }

            var siblings = _storageCache.FindSiblings(savedFav);

            foreach (var s in siblings)
            {
                s.IsFavorite = true;
                _storageCache.UpsertFile(s);
            }
            _storageCache.WriteToDisk();

            return savedFav as LaunchableItem;
        }

        public async Task RemoveFavorite(LaunchableItem file)
        {
            file.IsFavorite = false;
            _storageCache.UpsertFile(file);

            FileItem? fav = _storageCache
                .GetFavoriteFiles()
                .FirstOrDefault(f => f.Id == file.Id);

            if (fav is null) 
            {
                _alert.Publish($"The file {file.Name} is not a favorite."); 
                _log.InternalError($"The file {file.Name} is not a favorite.", _settings.StorageType.ToString());
                _storageCache.WriteToDisk();
                return;
            }
            var result = await _mediator.Send(new DeleteFileCommand(_settings.StorageType, fav.Path));

            if (!result.IsSuccess)
            {
                _alert.Publish($"There was an error untagging {file.Name} as a favorite.");
                _log.InternalError($"There was an error untagging {file.Name} as a favorite.", _settings.StorageType.ToString());
                _storageCache.WriteToDisk();
                return;
            }
            _storageCache.DeleteFile(fav.Path);

            FileItem? parent = _storageCache.FindParentFile(file);

            if (parent is not null) 
            {
                parent.IsFavorite = false;
                _storageCache.UpsertFile(parent);
            }
            var siblings = _storageCache.FindSiblings(file);

            foreach (var s in siblings)
            {
                s.IsFavorite = false;
                _storageCache.UpsertFile(s);
            }
            _filesDeleted.OnNext([fav]);
            _alert.Publish($"{fav.Path} was untagged as a favorite.");
            _storageCache.WriteToDisk();
        }

        public void MarkIncompatible(LaunchableItem launchItem)
        {
            launchItem.IsCompatible = false;
            _storageCache.UpsertFile(launchItem);
            _storageCache.WriteToDisk();
        }

        public void ClearCache() => _storageCache.ClearCache();
        public void ClearCache(DirectoryPath path) => _storageCache.DeleteDirectoryWithChildren(path);


        public async Task<IStorageCacheItem?> GetDirectory(DirectoryPath path)
        {
            var cacheItem = _storageCache.GetByDirPath(path);

            if (cacheItem != null)
            {
                return cacheItem;
            }

            var response = await _mediator.Send(new GetDirectoryRecursiveCommand(_settings.StorageType, path, false));

            var directoryResult = response.DirectoryContent.FirstOrDefault();

            if (directoryResult is null) 
            {
                _alert.Publish($"{path} was not found on the TR {_settings.StorageType} storage.");
                _log.InternalError($"{path} was not found on the TR {_settings.StorageType} storage.");
                return null;
            }

            var filteredContent = FilterBannedItems(directoryResult);

            if (filteredContent is null) return null;

            cacheItem = await SaveDirectoryToCache(filteredContent);

            _storageCache.WriteToDisk();
            return cacheItem;
        }

        private async Task<IStorageCacheItem> SaveDirectoryToCache(DirectoryContent dirContent)
        {
            IStorageCacheItem? cacheItem;
            var files = MapAndOrderFiles(dirContent);

            var playlistFile = dirContent.Files
                        .FirstOrDefault(f => f.Path.FileName == StorageHelper.Playlist_File_Name);

            if (playlistFile is not null)
            {
                var customResult = await _mediator.Send(new GetFileCommand(_settings.StorageType, playlistFile.Path));

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

            _storageCache.UpsertDirectory(dirContent.Path, cacheItem);
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
                SongItem s => _sidMetadata.EnrichSong(s),
                GameItem g => _gameMetadata.EnrichGame(g),
                HexItem h => HexItem.MapHexItem(h),
                ImageItem i => i,
                _ => file
            };
        }

        public void SaveFiles(IEnumerable<FileItem> files)
        {
            foreach (var f in files)
            {
                _storageCache.UpsertFile(f);
            }
            _storageCache.WriteToDisk();            
        }

        public async Task DeleteFile(FileItem file, TeensyStorageType storageType)
        {
            var deleteResult = await _mediator.Send(new DeleteFileCommand(storageType, file.Path));

            if (!deleteResult.IsSuccess && deleteResult.IsBusy) 
            {
                _alert.Publish("Make sure you have no running programs before deleting.");
                return;
            }

            _storageCache.DeleteFile(file.Path);

            _storageCache
                .GetFileByName(file.Name)
                .ForEach(f => f.IsFavorite = false);

            _storageCache.WriteToDisk();

            _filesDeleted.OnNext([file]);
        }
        public void Dispose() => _settingsSubscription?.Dispose();

        public LaunchableItem? GetRandomFile(StorageScope scope, DirectoryPath scopePath, params TeensyFileType[] fileTypes)
        {
            var excludePaths = GetExcludePaths();
            return _storageCache.GetRandomFile(scope, scopePath, excludePaths, fileTypes);
        }

        public IEnumerable<LaunchableItem> Search(string searchText, params TeensyFileType[] fileTypes)
        {
            var excludePaths = GetExcludePaths();

            return _storageCache.Search
            (
                searchText,
                excludePaths,
                _settings.SearchStopWords,
                _settings.SearchWeights,
                fileTypes
            );
        }

        private List<DirectoryPath> GetExcludePaths()
        {
            var excludePaths = StorageHelper.FavoritePaths;
            excludePaths.Add(new DirectoryPath(StorageHelper.Playlist_Path));

            return excludePaths.ToList();
        }

        public Task CacheAll() => CacheAll(new DirectoryPath(StorageHelper.Remote_Path_Root));

        public async Task CacheAll(DirectoryPath path)
        {
            if (path == new DirectoryPath(StorageHelper.Remote_Path_Root))
            {
                ClearCache();
            }
            else
            {
                ClearCache(path);
            }
            _alert.Publish($"Refreshing cache for {path} and all nested directories.");

            var getDirectoryCommand = new GetDirectoryRecursiveCommand
            (
                storageType: _settings.StorageType,
                path: path,
                recursive: true
            );
            var response = await _mediator.Send(getDirectoryCommand);

            if (!response.IsSuccess) return;

            _alert.Publish($"Enriching music and games.");

            await Task.Run(async () =>
            {
                foreach (var directory in response.DirectoryContent)
                {
                    if (directory is null) continue;

                    var filteredContent = FilterBannedItems(directory);

                    if (filteredContent is null) continue;

                    await SaveDirectoryToCache(filteredContent);
                }
                _storageCache.EnsureFavorites();
                _storageCache.EnsurePlaylists();
                _storageCache.WriteToDisk();
            });
            _alert.Publish($"Indexing completed for {_settings.StorageType} storage.");
        }

        private DirectoryContent? FilterBannedItems(DirectoryContent directoryContent)
        {
            var pathBanned = _settings.BannedDirectories.Any(d => directoryContent.Path.Value.Contains(d));
            if (pathBanned) return null;

            var filteredDirectories = directoryContent.Directories.Where(d => _settings.BannedDirectories.All(b => !d.Path.Value.Contains(b)));
            var filteredFiles = directoryContent.Files.Where(f => _settings.BannedFiles.All(b => !f.Name.Contains(b)));

            return new DirectoryContent
            {
                Path = directoryContent.Path,
                Directories = filteredDirectories.ToList(),
                Files = filteredFiles.ToList()
            };
        }

        public async Task CopyFiles(List<CopyFileItem> fileItems)
        {
            List<CopyFileResult> results = [];
            List<FileItem> filesAdded = [];

            foreach (var item in fileItems)
            {
                var targetFullPath = item.TargetPath.Combine(new FilePath(item.SourceItem.Name));

                var copyItem = new CopyFileCommand
                (
                    storageType: _settings.StorageType,
                    sourcePath: item.SourceItem.Path,
                    destPath: targetFullPath
                );
                var result = await _mediator.Send(copyItem);
                results.Add(result);

                if (!result.IsSuccess) continue;

                var newFile = CloneToFileItem(item.SourceItem);
                newFile.Path = targetFullPath;

                if (newFile.Path.Contains(StorageHelper.Favorites_Path)) 
                {   
                    newFile.IsFavorite = true;
                    var parentFile = _storageCache.FindParentFile(newFile);

                    if(parentFile is not null)
                    {
                        parentFile.IsFavorite = true;
                        newFile.ParentPath = parentFile.Path;
                        _storageCache.UpsertFile(parentFile);
                    }
                    var siblings = _storageCache.FindSiblings(newFile);
                    foreach (var sibling in siblings)
                    {
                        sibling.IsFavorite = true;
                        _storageCache.UpsertFile(sibling);
                    }
                }
                _storageCache.UpsertFile(newFile);
                filesAdded.Add(newFile);
            }
            if (results.Any(r => r.IsSuccess is false))
            {
                _alert.Publish($"There was an error copying files.");
                return;
            }
            _alert.Publish($"File(s) have been copied successfully.");

            _storageCache.WriteToDisk();
            _filesChanged.OnNext(filesAdded);
        }

        public FileItem MaybeEnsureFavoriteAndUpsert(LaunchableItem sourceFile, FilePath targetFullPath)
        {
            var favPaths = _settings.GetAllFavoritePaths();

            var newFile = CloneToFileItem(sourceFile);
            newFile.Path = targetFullPath;
            newFile.ParentPath = sourceFile.Path;

            if (favPaths.Any(fav => targetFullPath.Value.StartsWith(fav.Value, StringComparison.OrdinalIgnoreCase)))
            {
                sourceFile.IsFavorite = true;
                newFile.IsFavorite = true;
                _storageCache.UpsertFile(sourceFile);
            }
            else
            {
                newFile.IsFavorite = false;
            }
            _storageCache.UpsertFile(newFile);
            return newFile;
        }

        public int GetCacheSize() => _storageCache.GetCacheSize();

        public FileItem CloneToFileItem(LaunchableItem launchItem)
        {
            var clone = launchItem switch
            {
                SongItem s => s.Clone(),
                GameItem g => g.Clone(),
                HexItem h => h.Clone(),
                ImageItem image => image.Clone(),
                FileItem f => f.Clone(),
                _ => throw new TeensyException("Unknown file type")
            };
            return clone;
        }

        public async Task UpsertFiles(IEnumerable<FileItem> files)
        {
            var storagePath = files.First().Path.Directory;

            foreach (var f in files)
            {
                _storageCache.UpsertFile(f);
            }

            var directoryCache = _storageCache.GetByDirPath(storagePath);

            if (directoryCache is null)
            {
                _alert.Publish($"Unable to find the indexed directory: {storagePath}");
                return;
            }
            var customItems = directoryCache!.Files
                .Select(f => f.Custom)
                .Cast<PlaylistItem>()
                .Where(f => f is not null)
                .ToList();

            var playlist = new Playlist
            {
                Path = storagePath.Combine(new FilePath(StorageHelper.Playlist_File_Name)),
                Items = customItems
            };
            await TransferPlaylist(playlist);
            _storageCache.WriteToDisk();
            _filesChanged.OnNext(files);
        }

        private async Task TransferPlaylist(Playlist playlist)
        {
            var directoryPath = Path.Combine(
                Assembly.GetExecutingAssembly().GetPath(),
                StorageHelper.Temp_Path);

            var filePath = Path.Combine(directoryPath, StorageHelper.Playlist_File_Name);

            var playlistJson = LaunchableItemSerializer.Serialize(playlist);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(filePath, playlistJson);

            FileInfo fileInfo = new(filePath);

            var playlistTransferItem = new FileTransferItem(fileInfo, playlist.Path, _settings.StorageType);

            var result = await _mediator.Send(new SaveFilesCommand([playlistTransferItem]));

            if (!result.IsSuccess)
            {
                _alert.Publish("There was an issue transferring the playlist file to TeensyROM.");
                return;
            }

            var playlistItem = playlist.Items.FirstOrDefault(i => i.FilePath.Equals(playlist.Path));

            FileItem playlistFileItem = new()
            {
                Path = playlist.Path,
                Name = fileInfo.Name,
                Size = fileInfo.Length,
                Custom = playlistItem is not null ? playlistItem : new PlaylistItem
                {
                    FilePath = playlist.Path,

                },
                Description = "TeensyROM UI custom playlist file.",
                Creator = "TeensyROM UI",
                ReleaseInfo = "TeensyRom",
                Title = "Playlist File"
            };
            File.Delete(filePath);

            var existingItem = _storageCache.GetFileByPath(playlistFileItem.Path);

            _storageCache.UpsertFile(playlistFileItem);

            if (existingItem is not null)
            {
                _filesChanged.OnNext([playlistFileItem]);
            }
            else
            {
                _filesAdded.OnNext([playlistFileItem]);
            }
        }
    }
}