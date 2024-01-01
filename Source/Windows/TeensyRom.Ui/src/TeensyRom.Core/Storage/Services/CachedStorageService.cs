using MediatR;
using Newtonsoft.Json;
using System.Reflection;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Music;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public abstract class CachedStorageService<T> : ICachedStorageService<T> where T : FileItem
    {
        protected readonly ISettingsService _settingsService;
        protected readonly IMediator _mediator;
        protected TeensySettings? _settings;
        protected IDisposable? _settingsSubscription;
        protected const string _cacheFileName = "TeensyStorageCache.json";
        protected FileDirectoryCache _fileDirectoryCache = new();

        public CachedStorageService(ISettingsService settings, IMediator mediator)
        {
            _settingsService = settings;
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

        /// <summary>
        /// Override per storage file type
        /// </summary>
        /// <returns>Path to favorites</returns>
        protected abstract string GetFavoritesPath();
        protected abstract List<T> MapAndOrderFiles(DirectoryContent? directoryContent);
        public abstract Task<T?> SaveFavorite(T item);
        protected string GetFullCachePath() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", _cacheFileName);
        public void ClearCache()
        {
            _fileDirectoryCache.Clear();

            var cachePath = GetFullCachePath();

            if (!File.Exists(cachePath)) return;

            File.Delete(cachePath);
        }
        public void ClearCache(string path) => _fileDirectoryCache.DeleteDirectoryTree(path);
        protected void LoadCache()
        {
            var saveCacheEnabled = _settings?.SaveMusicCacheEnabled ?? false;

            if (!saveCacheEnabled) return;

            var cacheLocation = GetFullCachePath();

            if (!File.Exists(cacheLocation)) return;

            using var stream = File.Open(cacheLocation, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            var cacheFromDisk = JsonConvert.DeserializeObject<FileDirectoryCache>(content, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            });
            _fileDirectoryCache = cacheFromDisk;
        }
        protected void SaveCacheToDisk()
        {
            if (!_settings!.SaveMusicCacheEnabled) return;

            var cacheLocation = GetFullCachePath();

            File.WriteAllText(cacheLocation, JsonConvert.SerializeObject(_fileDirectoryCache, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            }));
        }
        public async Task<FileDirectory?> GetDirectory(string path)
        {
            var cacheItem = _fileDirectoryCache.GetByDirectory(path);

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

            cacheItem = new FileDirectory
            {
                Path = path,
                Directories = directories.ToList(),
                Files = songs.Cast<FileItem>().ToList()
            };

            _fileDirectoryCache.UpsertDirectory(path, cacheItem);
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
        public void Dispose() => _settingsSubscription?.Dispose();
    }
}