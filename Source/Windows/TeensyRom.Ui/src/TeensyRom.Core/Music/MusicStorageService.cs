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
        void Reset();        
        MusicDirectory? GetSongParentDirectory(string path);
        MusicDirectory? GetDirectory(string path);
        SongItem? SaveFavorite(SongItem song);
        void Dispose();
    }

    public class MusicStorageService : IDisposable, IMusicStorageService
    {        
        private readonly ISettingsService _settingsService;
        private readonly ISidMetadataService _metadataService;
        private readonly IGetDirectoryCommand _getDirectoryCommand;
        private readonly ICopyFileCommand _copyFileCommand;
        private TeensySettings _settings = new();
        private IDisposable? _settingsSubscription;
        private const string _cacheFileName = "TeensyStorageCache.json";        
        private MusicDirectoryCache _musicCache = new();

        public MusicStorageService(ISettingsService settings, ISidMetadataService metadataService, IGetDirectoryCommand getDirectoryCommand, ICopyFileCommand copyFileCommand)
        {
            _settingsService = settings;
            _metadataService = metadataService;
            _getDirectoryCommand = getDirectoryCommand;
            _copyFileCommand = copyFileCommand;
            LoadCacheFromDisk();
            _settingsSubscription = _settingsService.Settings.Subscribe(OnSettingsChanged);
        }

        private void OnSettingsChanged(TeensySettings settings)
        {
            _settings = settings;
        }

        private string GetFavoritesPath() => _settings
            .GetFileTypePath(TeensyFileType.Sid)
            .UnixPathCombine("/playlists/favorites");

        private string GetFullCachePath() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", _cacheFileName);

        public void Reset() => _musicCache.Clear();

        public SongItem? SaveFavorite(SongItem song)
        {
            var songFileName = song.Path.GetFileNameFromPath();            
            var targetPath = GetFavoritesPath().UnixPathCombine(songFileName);
            var copySuccess = _copyFileCommand.Execute(song.Path, targetPath);

            if (!copySuccess) return null;

            song.IsFavorite = true;
            var favSong = song.Clone();
            favSong.Path = targetPath;

            _musicCache.Upsert(song);
            _musicCache.Upsert(favSong);

            SaveCacheToDisk();

            return favSong;
        }

        private void LoadCacheFromDisk()
        {
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
            var cacheLocation = GetFullCachePath();

            File.WriteAllText(cacheLocation, JsonConvert.SerializeObject(_musicCache, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            }));
        }
        public MusicDirectory? GetSongParentDirectory(string path) => _musicCache.GetParentDirectory(path);
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
                Directories = directories.ToList(),
                Songs = songs
            };
            _musicCache.Insert(path, cacheItem);
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
