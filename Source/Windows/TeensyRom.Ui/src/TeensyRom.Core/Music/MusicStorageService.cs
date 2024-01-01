using MediatR;
using System.Transactions;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;

namespace TeensyRom.Core.Music
{
    public class MusicStorageService : CachedStorageService<SongItem>
    {        
        private readonly ISidMetadataService _metadataService;

        public MusicStorageService(ISettingsService settings, ISidMetadataService metadataService, IMediator mediator) : base(settings, mediator)
        {
            _metadataService = metadataService;
        }

        protected override string GetFavoritesPath() => _settings
            .GetFileTypePath(TeensyFileType.Sid)
            .UnixPathCombine("/playlists/favorites");

        public override async Task<SongItem?> SaveFavorite(SongItem song)
        {
            var songFileName = song.Path.GetFileNameFromPath();
            var targetPath = GetFavoritesPath().UnixPathCombine(songFileName);
            var result = await _mediator.Send(new CopyFileCommand
            {
                SourcePath = song.Path,
                DestPath = targetPath
            });

            if (!result.IsSuccess) return null;

            song.IsFavorite = true;
            var favSong = song.Clone();
            favSong.Path = targetPath;

            _fileDirectoryCache.UpsertFile(song);
            _fileDirectoryCache.UpsertFile(favSong);

            SaveCacheToDisk();

            return favSong;
        }

        protected override List<SongItem> MapAndOrderFiles(DirectoryContent? directoryContent)
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
                .ToList() ?? [];
        }
    }
}