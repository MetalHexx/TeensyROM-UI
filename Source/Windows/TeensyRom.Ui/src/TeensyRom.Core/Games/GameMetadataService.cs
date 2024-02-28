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

        public void EnrichGame(GameItem game)
        {
            var imageFileName = game.Name.ReplaceExtension(".png");

            var loadScreenLocalPath = Path.Combine(_localLoadingScreenPath, imageFileName);
            var screenshotLocalPath = Path.Combine(_localScreenshotPath, imageFileName);            

            game.Images.Add(new ViewableItemImage
            {
                Path = File.Exists(loadScreenLocalPath) ? loadScreenLocalPath : string.Empty,
                Source = GameConstants.OneLoad64
            });
            game.Images.Add(new ViewableItemImage
            {
                Path = File.Exists(screenshotLocalPath) ? screenshotLocalPath : string.Empty,
                Source = GameConstants.OneLoad64
            });
            game.Title = game.Name[..game.Name.LastIndexOf('.')];
        }
    }
}