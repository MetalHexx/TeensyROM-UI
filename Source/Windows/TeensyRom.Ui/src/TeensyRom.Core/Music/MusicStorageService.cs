using MediatR;
using Newtonsoft.Json;
using System.Reflection;
using System.Transactions;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public interface IMusicStorageService
    {
        void ClearCache();        
        void ClearCache(string path);
        MusicDirectory? GetDirectory(string path);        
        Task<SongItem?> SaveFavorite(SongItem song);
        void Dispose();
    }

    public class MusicStorageService : IDisposable, IMusicStorageService
    {        
        private readonly ISettingsService _settingsService;
        private readonly ISidMetadataService _metadataService;
        private readonly IGetDirectoryCommand _getDirectoryCommand;
        private readonly IMediator _mediator;
        private TeensySettings? _settings;
        private IDisposable? _settingsSubscription;
        private const string _cacheFileName = "TeensyStorageCache.json";        
        private MusicDirectoryCache _musicCache = new();

        public MusicStorageService(ISettingsService settings, ISidMetadataService metadataService, IGetDirectoryCommand getDirectoryCommand, IMediator mediator)
        {
            _settingsService = settings;
            _metadataService = metadataService;
            _getDirectoryCommand = getDirectoryCommand;
            _mediator = mediator;            
            _settingsSubscription = _settingsService.Settings.Subscribe(OnSettingsChanged);
        }

        private void OnSettingsChanged(TeensySettings newSettings)
        {
            if(_settings is null && newSettings.SaveMusicCacheEnabled)
            {
                _settings = newSettings;
                LoadCache();                
                return;
            }
            var shouldDeleteCache = _settings is not null
                && _settings.SaveMusicCacheEnabled
                && newSettings.SaveMusicCacheEnabled == false;

            _settings = newSettings;

            if (shouldDeleteCache)
            {
                ClearCache();
            }
        }

        private string GetFavoritesPath() => _settings
            .GetFileTypePath(TeensyFileType.Sid)
            .UnixPathCombine("/playlists/favorites");

        private string GetFullCachePath() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", _cacheFileName);

        public void ClearCache()
        {
            _musicCache.Clear();

            var cachePath = GetFullCachePath();

            if (!File.Exists(cachePath)) return;

            File.Delete(cachePath);
        }

        public void ClearCache(string path) => _musicCache.DeleteDirectoryTree(path);

        public async Task<SongItem?> SaveFavorite(SongItem song)
        {
            var songFileName = song.Path.GetFileNameFromPath();            
            var targetPath = GetFavoritesPath().UnixPathCombine(songFileName);
            var copySuccess = await _mediator.Send(new CopyFileRequest(song.Path, targetPath));

            if (!copySuccess) return null;

            song.IsFavorite = true;
            var favSong = song.Clone();
            favSong.Path = targetPath;

            _musicCache.UpsertSong(song);
            _musicCache.UpsertSong(favSong);

            SaveCacheToDisk();

            return favSong;
        }

        private void LoadCache()
        {
            var saveCacheEnabled = _settings?.SaveMusicCacheEnabled ?? false;
            
            if (!saveCacheEnabled) return;

            var cacheLocation = GetFullCachePath();

            if (!File.Exists(cacheLocation)) return;

            using var stream = File.Open(cacheLocation, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            var cacheFromDisk = JsonConvert.DeserializeObject<MusicDirectoryCache>(content, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            });
            _musicCache = cacheFromDisk;
        }
        
        private void SaveCacheToDisk()
        {
            if (!_settings!.SaveMusicCacheEnabled) return;

            var cacheLocation = GetFullCachePath();

            File.WriteAllText(cacheLocation, JsonConvert.SerializeObject(_musicCache, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            }));
        }
        public MusicDirectory? GetDirectory(string path)
        {
            var cacheItem = _musicCache.GetByDirectory(path);

            if (cacheItem != null) return cacheItem;

            var directoryContent = _getDirectoryCommand.Execute(path, 0, 5000); //TODO: Do something about this hardcoded take 5000

            if (directoryContent is null) return null;

            var songs = MapAndOrderSongs(directoryContent);
            var directories = MapAndOrderDirectories(directoryContent);

            cacheItem = new MusicDirectory
            {
                Path = path,
                Directories = directories.ToList(),
                Songs = songs
            };

            _musicCache.UpsertDirectory(path, cacheItem);
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
        private List<SongItem> MapAndOrderSongs(DirectoryContent? directoryContent)
        {
            return directoryContent?.Files
                .Select(file => new SongItem
                {
                    Name = file.Name,
                    Path = file.Path,
                    Size = file.Size
                })
                .Select(song => _metadataService.EnrichSong(song))
                .OrderBy(song => song.Name)
                .ToList() ?? new List<SongItem>();
        }
        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }
    }
}
