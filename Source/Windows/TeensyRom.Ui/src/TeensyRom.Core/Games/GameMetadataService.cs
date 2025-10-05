using System.Reactive.Linq;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Entities.Storage;
using System.Reflection;
using TeensyRom.Core.Common;
using System.Text.Json;

namespace TeensyRom.Core.Games
{
    public class GameMetadataService(ILoggingService log) : IGameMetadataService
    {  
        private string _gameArtPath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), GameConstants.Game_Image_Local_Path);        
        private string _localLoadingScreenPath => Path.Combine(_gameArtPath, GameConstants.Loading_Screen_Sub_Path);        
        private string _localScreenshotPath => Path.Combine(_gameArtPath, GameConstants.Screenshots_Sub_Path);
        private string _gameMetadataFilePath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), GameConstants.Game_Image_Metadata_File_Path);
        private List<ViewableItemImage> _gameMetadata = [];

        private List<ViewableItemImage> LoadGameMetadata()
        {
            if(File.Exists(_gameMetadataFilePath))
            {
                log.Internal($"Reading game metadata file: {_gameMetadataFilePath}");
                var fileMetadata = JsonSerializer.Deserialize<List<ViewableItemImage>>(File.ReadAllText(_gameMetadataFilePath));

                if(fileMetadata is not null && fileMetadata.Any())
                {
                    log.Internal($"Loaded {fileMetadata.Count} game metadata items from file.");
                    
                    // Populate BaseAssetPath for cached metadata that might be missing this property
                    foreach (var item in fileMetadata)
                    {
                        if (string.IsNullOrEmpty(item.BaseAssetPath))
                        {
                            var isLoadingScreen = item.Path.Contains(GameConstants.Loading_Screen_Sub_Path);
                            var basePath = isLoadingScreen
                                ? "/" + GameConstants.Game_Image_Local_Path.Replace('\\', '/') + "/" + GameConstants.Loading_Screen_Sub_Path + "/" 
                                : "/" + GameConstants.Game_Image_Local_Path.Replace('\\', '/') + "/" + GameConstants.Screenshots_Sub_Path + "/";
                            item.BaseAssetPath = basePath + item.FileName;
                        }
                    }
                    
                    return fileMetadata;
                }
            }
            var loadingScreens = Directory.GetFiles(_localLoadingScreenPath);
            var screenshots = Directory.GetFiles(_localScreenshotPath);

            log.Internal($"Found {loadingScreens.Length} loading screens and {screenshots.Length} screenshots.");

            var loadingScreenImages = loadingScreens.Select(f => new ViewableItemImage
            {
                FileName = Path.GetFileName(f),
                Path = f,
                BaseAssetPath = "/" + GameConstants.Game_Image_Local_Path.Replace('\\', '/') + "/" + GameConstants.Loading_Screen_Sub_Path + "/" + Path.GetFileName(f),
                Source = GameConstants.OneLoad64
            });

            var screenshotImages = screenshots.Select(f => new ViewableItemImage
            {
                FileName = Path.GetFileName(f),
                Path = f,
                BaseAssetPath = "/" + GameConstants.Game_Image_Local_Path.Replace('\\', '/') + "/" + GameConstants.Screenshots_Sub_Path + "/" + Path.GetFileName(f),
                Source = GameConstants.OneLoad64
            });

            var allScreenMetadata = loadingScreenImages
                .Concat(screenshotImages)
                .OrderBy(f => f.FileName)
                .ToList();

            log.Internal($"Mapped {allScreenMetadata.Count} game metadata items.");

            if (!Directory.Exists(Path.GetDirectoryName(_gameMetadataFilePath)))
            {
                log.Internal($"Creating directory for game metadata file: {Path.GetDirectoryName(_gameMetadataFilePath)}");
                Directory.CreateDirectory(Path.GetDirectoryName(_gameMetadataFilePath)!);
            }
            log.Internal($"Writing game metadata file: {_gameMetadataFilePath}");
            File.WriteAllText(_gameMetadataFilePath, JsonSerializer.Serialize(allScreenMetadata));

            if(File.Exists(_gameMetadataFilePath))
            {
                log.InternalSuccess($"Game metadata file written successfully.");
            }
            else
            {
                log.InternalError($"Failed to write game metadata file.");
            }
            return allScreenMetadata;
        }

        public GameItem EnrichGame(GameItem game)
        {
            if (!_gameMetadata.Any()) 
            {
                _gameMetadata = LoadGameMetadata();
            }
            var imageFileName = game.Name.ReplaceExtension(".png");

            var images = _gameMetadata.Where(m => m.FileName == imageFileName);

            game.Images.AddRange(images);

            game.MetadataSource = game.Images.Select(i => i.Source).FirstOrDefault() ?? string.Empty;

            return game;
        }
    }
}