using MediatR;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.DeleteFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Games;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public class CachedStorageService : ICachedStorageService
    {
        public IObservable<IEnumerable<IFileItem>> FilesDeleted => _filesDeleted.AsObservable();
        private Subject<IEnumerable<IFileItem>> _filesDeleted = new();
        public IObservable<IEnumerable<IFileItem>> FilesAdded => _filesAdded.AsObservable();
        private Subject<IEnumerable<IFileItem>> _filesAdded = new();

        public IObservable<IEnumerable<IFileItem>> FilesCopied => _filesCopied.AsObservable();
        private Subject<IEnumerable<IFileItem>> _filesCopied = new();

        public IObservable<System.Reactive.Unit> StorageReady => _storageCache.StorageReady;

        protected readonly ISettingsService _settingsService;
        protected readonly IAlertService _alert;
        private readonly IGameMetadataService _gameMetadata;
        private readonly ISidMetadataService _sidMetadata;
        private readonly IMediator _mediator;
        private TeensySettings _settings = null!;
        private IDisposable? _settingsSubscription;
        

        private IStorageCache _storageCache;
        
        public CachedStorageService(ISettingsService settingsService, IGameMetadataService gameMetadata, ISidMetadataService sidMetadata, IMediator mediator, IAlertService alert, IStorageCache storageCache)
        {
            _settingsService = settingsService;
            _gameMetadata = gameMetadata;
            _sidMetadata = sidMetadata;
            _mediator = mediator;
            _alert = alert;
            _storageCache = storageCache;
            _settings = settingsService.GetSettings();
            _settingsSubscription = _settingsService.Settings
                .Where(s => s is not null && s.LastCart is not null)
                .Subscribe(s => _settings = s);
        }
        
        public async Task<ILaunchableItem?> SaveFavorite(ILaunchableItem launchItem)
        {
            var favPath = _settings.GetFavoritePath(launchItem.FileType);

            var favCommand = new FavoriteFileCommand
            (
                storageType: _settings.StorageType, 
                sourcePath: launchItem.Path, 
                targetPath: favPath.UnixPathCombine(launchItem.Name)
            );

            var favoriteResult = await _mediator.Send(favCommand);

            if(!favoriteResult.IsSuccess)
            {
                _alert.Publish($"There was an error tagging {launchItem.Name} as favorite.");
                return null;
            }
            _alert.Publish($"{launchItem.Name} has been tagged as a favorite.");
            _alert.Publish($"A copy was placed in {favPath}.");

            if (!favoriteResult.IsSuccess) return null;

            var favItem = MaybeEnsureFavoriteAndStoreCache(launchItem, favPath.UnixPathCombine(launchItem.Name));

            _storageCache.WriteToDisk();

            return favItem as ILaunchableItem;
        }

        public async Task RemoveFavorite(ILaunchableItem file) 
        {
            IFileItem? sourceFile;
            IFileItem? favFile;

            if(string.IsNullOrWhiteSpace(file.FavChildPath))
            {
                favFile = file.Clone();
                sourceFile = _storageCache.GetFileByPath(file.FavParentPath);
            }
            else
            {
                sourceFile = file.Clone();
                favFile = _storageCache.GetFileByPath(file.FavChildPath);
            }
            if(sourceFile is not null)
            {
                sourceFile.IsFavorite = false;
                sourceFile.FavChildPath = string.Empty;
                _storageCache.UpsertFile(sourceFile);
            }
            if(favFile is null) return;

            favFile.IsFavorite = false;
            
            var result = await _mediator.Send(new DeleteFileCommand(_settings.StorageType, favFile.Path));

            if (!result.IsSuccess) return;

            ClearCache(favFile.Path.GetUnixParentPath());
            ClearCache(favFile.FavParentPath.GetUnixParentPath());

            favFile.Path = sourceFile?.Path ?? favFile.Path; //TODO: smell - this fixes a bug if favorite is re-favorited from play toolbar.

            _filesDeleted.OnNext([favFile]);
            _alert.Publish($"{favFile.Path} was untagged as a favorite.");            
        }

        public void MarkIncompatible(ILaunchableItem launchItem)
        {
            launchItem.IsCompatible = false;
            _storageCache.UpsertFile(launchItem);
            _storageCache.WriteToDisk();
        }

        public void ClearCache() => _storageCache.ClearCache();
        public void ClearCache(string path) => _storageCache.DeleteDirectoryWithChildren(path);

        
        public async Task<StorageCacheItem?> GetDirectory(string path)
        {   
            var cacheItem = _storageCache.GetByDirPath(path);

            if (cacheItem != null)
            {                
                return cacheItem;
            }

            var response = await _mediator.Send(new GetDirectoryCommand(_settings.StorageType, path));

            if (response.DirectoryContent is null) return null;  

            var filteredContent = FilterBannedItems(response.DirectoryContent);

            cacheItem = SaveDirectoryToCache(filteredContent);
            _storageCache.WriteToDisk();
            return cacheItem;
        }

        private StorageCacheItem SaveDirectoryToCache(DirectoryContent dirContent)
        {
            StorageCacheItem? cacheItem;
            var files = MapAndOrderFiles(dirContent);

            cacheItem = new StorageCacheItem
            {
                Path = dirContent.Path,
                Directories = dirContent.MapAndOrderDirectories(),
                Files = files
            };

            var favPaths = _settings.GetFavoritePaths();

            if (favPaths.Any(dirContent.Path.Contains)) FavCacheItems(cacheItem);

            _storageCache.UpsertDirectory(dirContent.Path, cacheItem);            
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
                SongItem s => _sidMetadata.EnrichSong(s),
                GameItem g => _gameMetadata.EnrichGame(g),
                HexItem h => HexItem.MapHexItem(h),
                ImageItem i => i,
                _ => file
            };
        }        

        public async Task<SaveFilesResult> SaveFiles(IEnumerable<FileTransferItem> files)
        {
            List<IFileItem> addedFiles = [];
            SaveFilesResult saveResults = new();

            var result = await _mediator.Send(new SaveFilesCommand(files.ToList()));         

            foreach (var f in result.SuccessfulFiles)
            {
                var storageItem = f.ToFileItem();

                if (storageItem is SongItem song) _sidMetadata.EnrichSong(song);
                if (storageItem is GameItem game) _gameMetadata.EnrichGame(game);
                if (storageItem is HexItem hex) HexItem.MapHexItem(hex);
                if (storageItem is FileItem file)
                {
                    _storageCache.UpsertFile(file);
                    addedFiles.Add(file);
                }
            }
            _storageCache.WriteToDisk();
            _filesAdded.OnNext(addedFiles);
            return saveResults;
        }

        public async Task DeleteFile(IFileItem file, TeensyStorageType storageType)
        {
            await _mediator.Send(new DeleteFileCommand(storageType, file.Path));

            _storageCache.DeleteFile(file.Path);

            _storageCache
                .GetFileByName(file.Name)
                .ForEach(f => f.IsFavorite = false);

            _filesDeleted.OnNext([file]);
        }
        public void Dispose() => _settingsSubscription?.Dispose();

        public ILaunchableItem? GetRandomFile(StorageScope scope, string scopePath, params TeensyFileType[] fileTypes)
        {
            return _storageCache.GetRandomFile(scope, scopePath, fileTypes);
        }

        public IEnumerable<ILaunchableItem> Search(string searchText, params TeensyFileType[] fileTypes)
        {
            return _storageCache.Search
            (
                searchText, 
                _settings.GetFavoritePaths(), 
                _settings.SearchStopWords, 
                _settings.SearchWeights, 
                fileTypes
            );
        }

        public Task CacheAll() => CacheAll(StorageConstants.Remote_Path_Root);

        public async Task CacheAll(string path)
        {
            if (path == StorageConstants.Remote_Path_Root)
            {
                ClearCache();
            }
            else 
            {
                ClearCache(path);
            }
            _alert.Publish($"Refreshing cache for {path} and all nested directories.");
            var response = await _mediator.Send(new GetDirectoryRecursiveCommand(_settings.StorageType, path));

            if (!response.IsSuccess) return;

            _alert.Publish($"Enriching music and games.");

            await Task.Run(() =>
            {
                foreach (var directory in response.DirectoryContent)
                {
                    if(directory is null) continue;

                    var filteredContent = FilterBannedItems(directory);

                    if(filteredContent is null) continue;

                    SaveDirectoryToCache(filteredContent);
                }
                _storageCache.EnsureFavorites(_settings.GetFavoritePaths());
                _storageCache.WriteToDisk();
            });
            _alert.Publish($"Indexing completed for {_settings.StorageType} storage.");
        }

        private DirectoryContent? FilterBannedItems(DirectoryContent directoryContent)
        {
            var pathBanned = _settings.BannedDirectories.Any(d => directoryContent.Path.Contains(d));
            if (pathBanned) return null;

            var filteredDirectories = directoryContent.Directories.Where(d => _settings.BannedDirectories.All(b => !d.Path.Contains(b)));
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
            List<IFileItem> filesAdded = [];

            foreach (var item in fileItems)
            {
                var targetFullPath = item.TargetPath.UnixPathCombine(item.SourceItem.Path.GetFileNameFromPath());

                var copyItem = new CopyFileCommand
                (
                    storageType: _settings.StorageType,
                    sourcePath: item.SourceItem.Path,
                    destPath: targetFullPath
                );
                var result = await _mediator.Send(copyItem);
                results.Add(result);

                if (!result.IsSuccess) continue;
                
                var newFile = MaybeEnsureFavoriteAndStoreCache(item.SourceItem, targetFullPath);
                filesAdded.Add(newFile);
            }
            if (results.Any(r => r.IsSuccess is false))
            {
                _alert.Publish($"There was an error copying files.");
                return;
            }
            _alert.Publish($"File(s) have been copied successfully.");

            _storageCache.WriteToDisk();
            _filesCopied.OnNext(filesAdded);
        }

        public IFileItem MaybeEnsureFavoriteAndStoreCache(ILaunchableItem sourceFile, string targetFullPath) 
        {
            var favPaths = _settings.GetAllFavoritePaths();

            var newFile = CloneToFileItem(sourceFile);
            newFile.Path = targetFullPath;
            newFile.FavParentPath = sourceFile.Path;

            if (favPaths.Any(fav => targetFullPath.StartsWith(fav, StringComparison.OrdinalIgnoreCase)))
            {
                sourceFile.FavChildPath = targetFullPath;
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

        public FileItem CloneToFileItem(ILaunchableItem launchItem)
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
    }
}