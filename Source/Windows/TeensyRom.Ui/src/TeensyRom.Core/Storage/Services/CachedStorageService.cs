using MediatR;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.DeleteFile;
using TeensyRom.Core.Commands.File.LaunchFile;
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
        public IObservable<string> FileAdded => _fileAdded.AsObservable();
        protected readonly ISettingsService _settingsService;
        private readonly IGameMetadataService _gameMetadata;
        private readonly ISidMetadataService _sidMetadata;
        private readonly IMediator _mediator;
        private readonly IAlertService _alert;
        private TeensySettings _settings = null!;
        private IDisposable? _settingsSubscription;
        private string _cacheFileName => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), StorageConstants.Cache_File_Path);
        private StorageCache _storageCache = null!;
        private Subject<string> _fileAdded = new();

        public CachedStorageService(ISettingsService settings, IGameMetadataService gameMetadata, ISidMetadataService sidMetadata, IMediator mediator, IAlertService alert)
        {
            _settingsService = settings;
            _gameMetadata = gameMetadata;
            _sidMetadata = sidMetadata;
            _mediator = mediator;
            _alert = alert;
            _settingsSubscription = _settingsService.Settings.Subscribe(OnSettingsChanged);
        }

        private void OnSettingsChanged(TeensySettings newSettings)
        {
            if(_storageCache is null)
            {
                _storageCache = new StorageCache(newSettings.BannedDirectories, newSettings.BannedFiles);
            }
            if (_settings is null && newSettings.SaveMusicCacheEnabled)
            {
                _settings = newSettings;                 
                LoadCache();
                return;
            }
            var shouldDeleteCache = _settings is not null
                && _settings.SaveMusicCacheEnabled
                && newSettings.SaveMusicCacheEnabled == false;

            _settings = newSettings;

            if (shouldDeleteCache) //if a user disabled cache saving, clear the cache
            {
                ClearCache();
            }
        }
        
        public async Task<ILaunchableItem?> SaveFavorite(ILaunchableItem launchItem)
        {
            var favPath = _settings.GetFavoritePath(launchItem.FileType);

            var favCommand = new FavoriteFileCommand
            {
                SourcePath = launchItem.Path,
                DestPath = favPath.UnixPathCombine(launchItem.Name)
            };

            var favoriteResult = await _mediator.Send(favCommand);

            if (favoriteResult.IsBusy)
            {
                _alert.Publish($"TR was reset to save the favorite.");
                _alert.Publish($"Re-launching {launchItem.Name}.");
                await _mediator.Send(new ResetCommand());                
                favoriteResult = await _mediator.Send(favCommand);
                await Task.Delay(5000);
                await _mediator.Send(new LaunchFileCommand { Path = launchItem.Path });
            }
            if(!favoriteResult.IsSuccess)
            {
                _alert.Publish($"There was an error tagging {launchItem.Name} as favorite.");
                return null;
            }
            _alert.Publish($"{launchItem.Name} has been tagged as a favorite.");
            _alert.Publish($"A copy was placed in {favPath}.");

            var favItem = launchItem switch
            {
                SongItem s => s.Clone(),
                GameItem g => g.Clone(),
                FileItem f => f.Clone(),                
                _ => throw new TeensyException("Unknown file type")
            };

            if (!favoriteResult.IsSuccess) return null;

            launchItem.IsFavorite = true;
            favItem.IsFavorite = true;
            
            favItem.Path = favPath.UnixPathCombine(favItem.Name);

            _storageCache.UpsertFile(launchItem);
            _storageCache.UpsertFile(favItem);

            SaveCacheToDisk();

            return favItem as ILaunchableItem;
        }

        public void MarkIncompatible(ILaunchableItem launchItem)
        {
            launchItem.IsCompatible = false;
            _storageCache.UpsertFile(launchItem);

            SaveCacheToDisk();
        }

        public void ClearCache()
        {
            _storageCache.Clear();

            if (!File.Exists(_cacheFileName)) return;

            File.Delete(_cacheFileName);
        }
        public void ClearCache(string path) => _storageCache.DeleteDirectoryWithChildren(path);
        private void LoadCache()
        {
            var saveCacheEnabled = _settings?.SaveMusicCacheEnabled ?? false;

            if (!saveCacheEnabled) return;


            if (!File.Exists(_cacheFileName)) return;
            LoadCacheFromDisk(_cacheFileName);
            EnsureFavorites();
            SaveCacheToDisk();
        }

        private void LoadCacheFromDisk(string cacheLocation)
        {
            using var stream = File.Open(cacheLocation, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            var cacheFromDisk = JsonConvert.DeserializeObject<StorageCache>(content, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            });
            cacheFromDisk?.SetBanLists(_settings.BannedDirectories, _settings.BannedFiles);

            if(cacheFromDisk is null) return;

            _storageCache = cacheFromDisk;
        }

        public void EnsureFavorites()
        {
            List<ILaunchableItem> favsToFavorite = GetFavoriteItemsFromCache();

            favsToFavorite.ForEach(f => f.IsFavorite = true);

            _storageCache
                .SelectMany(c => c.Value.Files)
                .Where(f => !f.IsFavorite && favsToFavorite.Any(fav => fav.Name == f.Name))
                .ToList()
                .ForEach(f => f.IsFavorite = true);
        }

        public List<ILaunchableItem> GetFavoriteItemsFromCache()
        {
            List<ILaunchableItem> favs = [];

            foreach (var target in _settings.GetFavoritePaths())
            {
                favs.AddRange(_storageCache
                    .Where(c => c.Key.Contains(target))
                    .SelectMany(c => c.Value.Files)
                    .ToList()
                    .Cast<ILaunchableItem>());
            }
            return favs;
        }

        private void SaveCacheToDisk()
        {
            if (!_settings!.SaveMusicCacheEnabled) return;

            if (!Directory.Exists(Path.GetDirectoryName(_cacheFileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_cacheFileName)!);
            }

            File.WriteAllText(_cacheFileName, JsonConvert.SerializeObject(_storageCache, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            }));
        }
        public async Task<StorageCacheItem?> GetDirectory(string path)
        {
            var cacheItem = _storageCache.GetByDirPath(path);

            if (cacheItem != null)
            {                
                return cacheItem;
            }

            var response = await _mediator.Send(new GetDirectoryCommand
            {
                Path = path
            });

            if (response.DirectoryContent is null) return null;  
            

            cacheItem = SaveDirectoryToCache(response.DirectoryContent);
            SaveCacheToDisk();
            return cacheItem;
        }

        private StorageCacheItem SaveDirectoryToCache(DirectoryContent dirContent)
        {
            StorageCacheItem? cacheItem;
            var files = MapAndOrderFiles(dirContent);
            var directories = MapAndOrderDirectories(dirContent);

            cacheItem = new StorageCacheItem
            {
                Path = dirContent.Path,
                Directories = directories.ToList(),
                Files = files
            };

            var favPaths = _settings.GetFavoritePaths();

            if (favPaths.Any(dirContent.Path.Contains)) FavCacheItems(cacheItem);

            _storageCache.UpsertDirectory(dirContent.Path, cacheItem);            
            return cacheItem;
        }

        private static void FavCacheItems(StorageCacheItem cacheItem) => cacheItem.Files.ForEach(f => f.IsFavorite = true);

        private static IEnumerable<DirectoryItem> MapAndOrderDirectories(DirectoryContent? directoryContent)
        {
            return directoryContent?.Directories
                .Select(d => new DirectoryItem
                {
                    Name = d.Name,
                    Path = d.Path
                })
                .OrderBy(d => d.Name)
                .ToList() ?? new List<DirectoryItem>();
        }

        private List<IFileItem> MapAndOrderFiles(DirectoryContent? directoryContent)
        {
            return directoryContent?.Files
                .Select(file =>
                {
                    if (file.FileType is TeensyFileType.Sid) 
                    {
                        var song = new SongItem { Name = file.Name, Title = file.Name, Path = file.Path, Size = file.Size };
                        _sidMetadata.EnrichSong(song);
                        return song;
                    }
                    if (file.FileType is TeensyFileType.Crt or TeensyFileType.Prg)
                    {
                        var game = new GameItem
                        {
                            Name = file.Name,
                            Path = file.Path,
                            Size = file.Size
                        };
                        _gameMetadata.EnrichGame(game);
                        return game;
                    }
                    return file;
                })
            .OrderBy(file => file.Name)
            .ToList() ?? [];
        }

        public async Task<int> SaveFiles(IEnumerable<TeensyFileInfo> files)
        {
            var dupeCount = 0;

            foreach (var f in files)
            {
                var isDupe = _storageCache.GetFileByPath(f.TargetPath.UnixPathCombine(f.Name)) is not null;

                if (isDupe)
                {
                    dupeCount++;
                    continue;
                }

                var result = await _mediator.Send(new SaveFileCommand
                {
                    File = f
                });
                if (!result.IsSuccess) return dupeCount;

                var storageItem = f.ToStorageItem();

                if (storageItem is SongItem song) _sidMetadata.EnrichSong(song);
                if (storageItem is GameItem game) _gameMetadata.EnrichGame(game);
                if (storageItem is FileItem file)
                {
                    _storageCache.UpsertFile(file);                    
                }
                _fileAdded.OnNext(storageItem.Path);
            }
            SaveCacheToDisk();
            return files.Count() - dupeCount;
        }

        public async Task QueuedSaveFile(TeensyFileInfo fileInfo)
        {
            var result = await _mediator.Send(new QueuedSaveFileCommand
            {
                File = fileInfo
            });
            if (!result.IsSuccess) return;

            var storageItem = fileInfo.ToStorageItem();

            if (storageItem is SongItem song) _sidMetadata.EnrichSong(song);
            if (storageItem is GameItem game) _gameMetadata.EnrichGame(game);
            if (storageItem is FileItem file) 
            {
                _storageCache.UpsertFile(file);
                SaveCacheToDisk();
            }

            _fileAdded.OnNext(storageItem.Path);
        }

        public async Task DeleteFile(IFileItem file, TeensyStorageType storageType)
        {      
            await _mediator.Send(new DeleteFileCommand 
            { 
                Path = file.Path, 
                StorageType = storageType 
            });
            _storageCache.DeleteFile(file.Path);

            _storageCache
                .GetFileByName(file.Name)
                .ForEach(f => f.IsFavorite = false);
        }
        public void Dispose() => _settingsSubscription?.Dispose();

        public ILaunchableItem? GetRandomFile(params TeensyFileType[] fileTypes) 
        {
            if (fileTypes.Length == 0)
            {
                fileTypes = TeensyFileTypeExtensions.GetLaunchFileTypes();
            }
            var selection = _storageCache
                .SelectMany(c => c.Value.Files)                
                .Where(f => fileTypes.Contains(f.FileType))
                .OfType<ILaunchableItem>()                
                .ToArray();

            if (selection.Length == 0) return null;

            return selection[new Random().Next(selection.Length - 1)];
        }

        public IEnumerable<ILaunchableItem> Search(string searchText, params TeensyFileType[] fileTypes)
        {
            var searchTerms = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return _storageCache
        .Where(NotFavoriteFilter)
        .SelectMany(c => c.Value.Files)
        .Where(f => fileTypes.Contains(f.FileType))
        .OfType<ILaunchableItem>()
        .Select(file => new
        {
            File = file,
            Score = searchTerms.Sum(term =>
                (file.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ? _settings.SearchWeights.Title : 0) + 
                (file.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ? _settings.SearchWeights.FileName : 0) + 
                (file.Creator.Contains(term, StringComparison.OrdinalIgnoreCase) ? _settings.SearchWeights.Creator : 0) +
                (file.Path.Contains(term, StringComparison.OrdinalIgnoreCase) ? _settings.SearchWeights.FilePath: 0) +
                (file.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ? _settings.SearchWeights.Description : 0))
        })
        .Where(result => result.Score > 0)
        .OrderByDescending(result => result.Score)
        .ThenBy(result => result.File.Title)
        .Select(result => result.File);
        }

        Func<KeyValuePair<string, StorageCacheItem>, bool> NotFavoriteFilter => 
            kvp => !_settings
                .GetFavoritePaths()
                .Select(p => p.RemoveLeadingAndTrailingSlash())
                .Any(favPath => kvp.Key.Contains(favPath));

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
            var response = await _mediator.Send(new GetDirectoryRecursiveCommand() { Path = path });

            if (!response.IsSuccess) return;

            _alert.Publish($"Enriching music and games.");

            await Task.Run(() =>
            {
                foreach (var directory in response.DirectoryContent)
                {
                    if (directory is not null)
                    {
                        SaveDirectoryToCache(directory);
                    }
                }
                EnsureFavorites();
                SaveCacheToDisk();
            });
            _alert.Publish($"Download completed for {_settings.TargetType} storage.");
        }
    }
}