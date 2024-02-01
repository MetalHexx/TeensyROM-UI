using MediatR;
using MediatR.Pipeline;
using Newtonsoft.Json;
using System.IO;
using System.Reactive.Linq;
using System.Runtime;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using System.Windows;
using System.Reflection;
using TeensyRom.Core.Common;
using TeensyRom.Core.Commands.GetFile;
using System.Collections;
using System.Drawing;

namespace TeensyRom.Core.Games
{
    public class GameMetadataService : IGameMetadataService
    {
        
        private readonly IMediator _mediator;
        private readonly IAlertService _alert;
        private TeensySettings _settings;
        private IDisposable _settingsSubscription;
        private GameCache? _gameCache;

        private readonly string _gameDataPath = "TeensyGameMetadata.json";
        private string _gameArtPath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), @"Games\Art");        
        private string _localLoadingScreenPath => Path.Combine(_gameArtPath, "LoadingScreens");        
        private string _localScreenshotPath => Path.Combine(_gameArtPath, "Screenshots");        
        private string GetGameArtPath(string filename, string parentPath) => Path.Combine(parentPath, filename.ReplaceExtension(".png"));

        public GameMetadataService(IMediator mediator, ISerialStateContext serialContext, ISettingsService settingsService, IAlertService alert)
        {
            _mediator = mediator;
            _alert = alert;

            _settingsSubscription = settingsService.Settings
                .Do(settings => _settings = settings)
                .Select(imagePaths => (imagePaths.GameLoadingScreenPath, imagePaths.GameScreenshotPath))
                .DistinctUntilChanged()
                .Do(_ => LoadCacheFromDisk())
                .CombineLatest(serialContext.CurrentState, (imagePaths, serial) => (imagePaths, serial))
                .Where(state => state.serial is SerialConnectedState)
                .Select(state => state.imagePaths)
                .Skip(2)
                .DistinctUntilChanged()
                .Subscribe(async state => await RefreshGameCache());
        }

        public async void GetGameScreens(GameItem game)
        {
            EnsureArtDirectories();

            await GetAndStoreFile(
                remotePath: game.Screens.LoadingScreenRemotePath, 
                localPath: game.Screens.LoadingScreenLocalPath, 
                storageType: TeensyStorageType.SD);


            await GetAndStoreFile(
                remotePath: game.Screens.ScreenshotRemotePath,
                localPath: game.Screens.ScreenshotLocalPath,
                storageType: TeensyStorageType.SD);
        }

        private async Task GetAndStoreFile(string remotePath, string localPath, TeensyStorageType storageType)
        {
            if (string.IsNullOrWhiteSpace(remotePath) || string.IsNullOrWhiteSpace(localPath)) return;

            if (File.Exists(localPath)) return;

            var result = await _mediator.Send(new GetFileCommand
            {
                FilePath = remotePath,
                StorageType = storageType
            });
            if (result.IsSuccess)
            {
                File.WriteAllBytes(localPath, result.FileData);
            }
        }

        private void EnsureArtDirectories()
        {
            if (!Directory.Exists(_localLoadingScreenPath))
            {
                Directory.CreateDirectory(_localLoadingScreenPath);
            }
            if (!Directory.Exists(_localScreenshotPath))
            {
                Directory.CreateDirectory(_localScreenshotPath);
            }
        }

        public void EnrichGame(GameItem game)
        {
            var imageFilename = game.Name.ReplaceExtension(".png");

            var loadScreenLocalPath = Path.Combine(_localLoadingScreenPath, imageFilename);
            var screenshotLocalPath = Path.Combine(_localScreenshotPath, imageFilename);            

            var loadingScreen = _gameCache?.LoadingScreenCache
                .Where(f => f.Name.Equals(imageFilename, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            var screenshot = _gameCache?.ScreenshotCache
                .Where(f => f.Name.Equals(imageFilename, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            game.Screens.LoadingScreenLocalPath = loadingScreen?.LocalPath ?? string.Empty;
            game.Screens.LoadingScreenRemotePath = loadingScreen?.RemotePath ?? string.Empty;
            game.Screens.ScreenshotLocalPath = screenshot?.LocalPath ?? string.Empty;
            game.Screens.ScreenshotRemotePath = screenshot?.RemotePath ?? string.Empty;
        }

        private void LoadCacheFromDisk()
        {
            if (!File.Exists(GetFullCachePath())) return;

            using var stream = File.Open(_gameDataPath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            var cacheFromDisk = JsonConvert.DeserializeObject<GameCache>(content, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            });
            _gameCache = cacheFromDisk;
        }

        private void SaveCacheToDisk()
        {
            if (!_settings!.SaveMusicCacheEnabled) return;

            var cacheLocation = GetFullCachePath();

            File.WriteAllText(cacheLocation, JsonConvert.SerializeObject(_gameCache, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            }));
        }

        private string GetFullCachePath() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", _gameDataPath);

        private async Task RefreshGameCache()
        {
            var imagePathsChanged =
                !_settings.GameLoadingScreenPath.Equals(_gameCache?.LoadingScreenImagePath) ||
                !_settings.GameScreenshotPath.Equals(_gameCache?.ScreenshotImagePath);

            if (!imagePathsChanged && _gameCache is not null) return;


            var hasPathsConfigured = !string.IsNullOrWhiteSpace(_settings.GameLoadingScreenPath) || !string.IsNullOrWhiteSpace(_settings.GameScreenshotPath);

            if (!hasPathsConfigured) return;
                
            _alert.Publish($"Fetching game metadata.");

            _gameCache = new GameCache
            {
                LoadingScreenImagePath = _settings.GameLoadingScreenPath,
                ScreenshotImagePath = _settings.GameScreenshotPath,
                LoadingScreenCache = await GetGameCacheFromTR(_settings.GameLoadingScreenPath, GameMetadataType.LoadingScreen),
                ScreenshotCache = await GetGameCacheFromTR(_settings.GameScreenshotPath, GameMetadataType.GamePlayScreen),
            };
            SaveCacheToDisk();
        }

        private async Task<List<GameCacheItem>> GetGameCacheFromTR(string path, GameMetadataType type)
        {
            if (string.IsNullOrWhiteSpace(path)) return [];

            var response = await _mediator.Send(new GetDirectoryCommand
            {
                Path = path
            });
            if (!response.IsSuccess)
            {
                _alert.Publish($"There was an issue receiving game metadata at {path}.");
                return [];
            }
            if (response.DirectoryContent?.Files is null || response.DirectoryContent.Files.Count == 0)
            {
                _alert.Publish($"No game metadata found at {path}.");
                return [];
            }
            return response.DirectoryContent.Files.Select(f => new GameCacheItem
            {
                Name = f.Name,
                RemotePath = f.Path,
                LocalPath = type is GameMetadataType.GamePlayScreen 
                    ? GetGameArtPath(f.Name, _localScreenshotPath)
                    : GetGameArtPath(f.Name, _localLoadingScreenPath),
                Type = type
            }).ToList();
        }
    }
}