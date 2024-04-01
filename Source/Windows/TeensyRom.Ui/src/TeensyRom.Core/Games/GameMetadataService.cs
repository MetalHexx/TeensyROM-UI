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
        private string _gameArtPath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), GameConstants.Game_Image_Local_Path);        
        private string _localLoadingScreenPath => Path.Combine(_gameArtPath, GameConstants.Loading_Screen_Sub_Path);        
        private string _localScreenshotPath => Path.Combine(_gameArtPath, GameConstants.Screenshots_Sub_Path);
        private string _gameMetadataFilePath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), GameConstants.Game_Image_Metadata_File_Path);
        private List<ViewableItemImage> _gameMetadata = [];

        private List<ViewableItemImage> LoadGameMetadata()
        {
            if(File.Exists(_gameMetadataFilePath))
            {
                var fileMetadata = JsonConvert.DeserializeObject<List<ViewableItemImage>>(File.ReadAllText(GameConstants.Game_Image_Metadata_File_Path));

                if(fileMetadata is not null && fileMetadata.Any())
                {
                    return fileMetadata;
                }
            }
            var loadingScreens = Directory.GetFiles(_localLoadingScreenPath);
            var screenshots = Directory.GetFiles(_localScreenshotPath);

            var allScreenMetadata = loadingScreens
                .Concat(screenshots)
                .ToList()
                .Select(f => new ViewableItemImage
                {
                    FileName = Path.GetFileName(f),
                    Path = f,
                    Source = GameConstants.OneLoad64
                })
                .OrderBy(f => f.FileName)
                .ToList();

            if (!Directory.Exists(Path.GetDirectoryName(_gameMetadataFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_gameMetadataFilePath)!);
            }
            File.WriteAllText(_gameMetadataFilePath, JsonConvert.SerializeObject(allScreenMetadata));
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

            return game;
        }
    }
}