using MediatR;
using Newtonsoft.Json;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music;
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
        public void ClearCache(string path) => _storageCache.DeleteWithChildren(path);
        private void LoadCache()
        {
            var saveCacheEnabled = _settings?.SaveMusicCacheEnabled ?? false;

            if (!saveCacheEnabled) return;

            var cacheLocation = GetFullCachePath();

            if (!File.Exists(cacheLocation)) return;

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

            if (cacheItem != null) return cacheItem;

            var response = await _mediator.Send(new GetDirectoryCommand
            {
                Path = path,
                Skip = 0,
                Take = 5000 //TODO: Do something about this hardcoded take 5000
            });

            if (response is null) return null;

            var songs = MapAndOrderFiles(response.DirectoryContent);
            var directories = MapAndOrderDirectories(response.DirectoryContent);

            cacheItem = new StorageCacheItem
            {
                Path = path,
                Directories = directories.ToList(),
                Files = songs.Cast<FileItem>().ToList()
            };

            _storageCache.UpsertDirectory(path, cacheItem);
            SaveCacheToDisk();

            return cacheItem;
        }
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
        public void Dispose() => _settingsSubscription?.Dispose();
    }
}