using MediatR;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.DeleteFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public class CachedStorageService : ICachedStorageService
    {
        public IObservable<string> DirectoryUpdated => _directoryUpdated.AsObservable();
        protected readonly ISettingsService _settingsService;
        private readonly ISidMetadataService _metadataService;
        private readonly IMediator _mediator;
        private TeensySettings _settings;
        private IDisposable? _settingsSubscription;
        private const string _cacheFileName = "TeensyStorageCache.json";
        private StorageCache _storageCache = new();
        private Subject<string> _directoryUpdated = new();

        public CachedStorageService(ISettingsService settings, ISidMetadataService metadataService, IMediator mediator)
        {
            _settingsService = settings;
            _metadataService = metadataService;
            _mediator = mediator;
            _settingsSubscription = _settingsService.Settings.Subscribe(OnSettingsChanged);
        }

        private void OnSettingsChanged(TeensySettings newSettings)
        {
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
        
        public async Task<FileItem?> SaveFavorite(FileItem fileItem)
        {
            var favPath = _settings.GetFavoritePath(fileItem.FileType);

            var result = await _mediator.Send(new CopyFileCommand
            {
                SourcePath = fileItem.Path,
                DestPath = favPath.UnixPathCombine(fileItem.Name)
            });

            var favItem = fileItem switch
            {
                SongItem s => s.Clone(),
                FileItem f => f.Clone(),
                _ => throw new TeensyException("Unknown file type")
            };

            if (!result.IsSuccess) return null;

            fileItem.IsFavorite = true;
            favItem.IsFavorite = true;
            
            favItem.Path = favPath.UnixPathCombine(favItem.Name);

            _storageCache.UpsertFile(fileItem);
            _storageCache.UpsertFile(favItem);

            SaveCacheToDisk();

            return favItem;
        }
        private string GetFullCachePath() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", _cacheFileName);
        public void ClearCache()
        {
            _storageCache.Clear();

            var cachePath = GetFullCachePath();

            if (!File.Exists(cachePath)) return;

            File.Delete(cachePath);
        }
        public void ClearCache(string path) => _storageCache.DeleteDirectoryWithChildren(path);
        private void LoadCache()
        {
            var saveCacheEnabled = _settings?.SaveMusicCacheEnabled ?? false;

            if (!saveCacheEnabled) return;

            var cacheLocation = GetFullCachePath();

            if (!File.Exists(cacheLocation)) return;
            LoadCacheFromDisk(cacheLocation);
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
            _storageCache = cacheFromDisk;
        }

        public void EnsureFavorites()
        {
            List<FileItem> favsToFavorite = GetFavoriteItemsFromCache();

            favsToFavorite.ForEach(f => f.IsFavorite = true);

            _storageCache
                .SelectMany(c => c.Value.Files)
                .Where(f => !f.IsFavorite && favsToFavorite.Any(fav => fav.Name == f.Name))
                .ToList()
                .ForEach(f => f.IsFavorite = true);
        }

        public List<FileItem> GetFavoriteItemsFromCache()
        {
            List<FileItem> favs = [];

            foreach (var target in _settings.GetFavoritePaths())
            {
                favs.AddRange(_storageCache
                    .Where(c => c.Key.Contains(target))
                    .SelectMany(c => c.Value.Files)
                    .ToList());
            }
            return favs;
        }

        private void SaveCacheToDisk()
        {
            if (!_settings!.SaveMusicCacheEnabled) return;

            var cacheLocation = GetFullCachePath();

            File.WriteAllText(cacheLocation, JsonConvert.SerializeObject(_storageCache, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            }));
        }
        public async Task<StorageCacheItem?> GetDirectory(string path)
        {
            var cacheItem = _storageCache.Get(path);

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
            return cacheItem;
        }

        private StorageCacheItem SaveDirectoryToCache(DirectoryContent dirContent)
        {
            StorageCacheItem? cacheItem;
            var songs = MapAndOrderFiles(dirContent);
            var directories = MapAndOrderDirectories(dirContent);

            cacheItem = new StorageCacheItem
            {
                Path = dirContent.Path,
                Directories = directories.ToList(),
                Files = songs.Cast<FileItem>().ToList()
            };

            var favPaths = _settings.GetFavoritePaths();

            if (favPaths.Any(dirContent.Path.Contains)) FavCacheItems(cacheItem);

            _storageCache.UpsertDirectory(dirContent.Path, cacheItem);            
            return cacheItem;
        }

        private static void FavCacheItems(StorageCacheItem? cacheItem) => cacheItem.Files.ForEach(f => f.IsFavorite = true);

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

        private List<FileItem> MapAndOrderFiles(DirectoryContent? directoryContent)
        {
            return directoryContent?.Files
                .Select(file =>
                {
                    if (file.FileType is not TeensyFileType.Sid) return file;

                    var song = new SongItem
                    {
                        Name = file.Name,
                        Path = file.Path,
                        Size = file.Size,
                    };
                    _metadataService.EnrichSong(song);
                    return song;
                })
                .OrderBy(song => song.Name)
                .ToList() ?? [];
        }

        public async Task SaveFile(TeensyFileInfo fileInfo)
        {
            var result = await _mediator.Send(new SaveFileCommand
            {
                File = fileInfo
            });
            if (!result.IsSuccess) return;

            var storageItem = fileInfo.ToStorageItem();

            if(storageItem is SongItem song) _metadataService.EnrichSong(song);
            if(storageItem is FileItem file) _storageCache.UpsertFile(file); 

            _directoryUpdated.OnNext(storageItem.Path);
        }

        public async Task DeleteFile(FileItem file, TeensyStorageType storageType)
        {
            await _mediator.Send(new DeleteFileCommand 
            { 
                Path = file.Path, 
                StorageType = storageType 
            });
            _storageCache.DeleteFile(file.Path);

            _storageCache
                .FindFile(file.Name)
                .ForEach(f => f.IsFavorite = false);
        }
        public void Dispose() => _settingsSubscription?.Dispose();

        public FileItem? GetRandomFile(params TeensyFileType[] fileTypes) 
        {
            if (fileTypes.Length == 0)
            {
                fileTypes = TeensyFileTypeExtensions.GetLaunchFileTypes();
            }
            var selection = _storageCache.SelectMany(c => c.Value.Files)
                .Where(f => fileTypes.Contains(f.FileType))
                .ToArray();

            return selection[new Random().Next(selection.Length - 1)];
        }
        public IEnumerable<SongItem> SearchMusic(string searchText, int maxNumResults = 250)
        {
            var searchTerms = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return _storageCache
                .SelectMany(c => c.Value.Files)
                .OfType<SongItem>()
                .Select(song => new
                {
                    Song = song,
                    Score = searchTerms.Count(term =>
                        song.ArtistName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        song.SongName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        song.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        song.Path.Contains(term, StringComparison.OrdinalIgnoreCase)
                    )
                })
                .Where(result => result.Score > 0)
                .OrderBy(result => result.Song.SongName)
                .OrderByDescending(result => result.Score)
                .Select(result => result.Song)
                .Take(maxNumResults);
        }

        public async Task CacheAll()
        {
            var allContent = await _mediator.Send(new GetDirectoryRecursiveCommand() { Path = "/" });

            foreach (var directory in allContent.DirectoryContent)
            {
                if(directory is not null)
                {
                    SaveDirectoryToCache(directory);
                }
            }
            EnsureFavorites();
            SaveCacheToDisk();
        }
    }
}