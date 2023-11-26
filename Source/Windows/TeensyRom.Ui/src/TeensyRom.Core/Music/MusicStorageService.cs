using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Music
{
    public interface IMusicService
    {
        void Reset();
        void Dispose();
        MusicDirectory? GetByDirectory(string path);
        MusicDirectory? GetSongParentDirectory(string path);
        MusicDirectory? GetDirectory(string path);
    }

    public class MusicStorageService : IDisposable, IMusicService
    {
        private readonly MusicDirectoryCache _musicCache = new();
        private readonly ISettingsService _settingsService;
        private readonly ISidMetadataService _metadataService;
        private readonly IGetDirectoryCommand _getDirectoryCommand;
        private TeensySettings _settings = new();
        private IDisposable? _settingsSubscription;

        public MusicStorageService(ISettingsService settings, ISidMetadataService metadataService, IGetDirectoryCommand getDirectoryCommand)
        {
            _settingsService = settings;
            _metadataService = metadataService;
            _getDirectoryCommand = getDirectoryCommand;
            _settingsSubscription = _settingsService.Settings.Subscribe(settings => _settings = settings);
        }

        public void Reset() => _musicCache.Clear();
        public MusicDirectory? GetByDirectory(string path) => _musicCache.GetByDirectory(path);
        public MusicDirectory? GetSongParentDirectory(string path) => _musicCache.GetBySong(path);
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
                .Select(song =>
                {
                    var defaultSidPath = _settings.GetFileTypePath(TeensyFileType.Sid);
                    var trimmedPath = song.Path.Replace($"{defaultSidPath}/hvsc", "");
                    return _metadataService.EnrichSong(song, trimmedPath);
                })
                .OrderBy(song => song.Name)
                .ToList() ?? new List<SongItem>();
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }
    }
}
