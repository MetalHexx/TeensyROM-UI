using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common;
using TeensyRom.Core.Games;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Games.State;

namespace TeensyRom.Ui.Features.Games.GameInfo
{
    public class GameInfoViewModel : ReactiveObject
    {
        [ObservableAsProperty] public string LoadingScreenPath { get; } = string.Empty;
        [ObservableAsProperty] public string ScreenshotPath { get; } = string.Empty;

        public GameInfoViewModel(IGameState gameState, IGameMetadataService gameMetadata)
        {
            var enrichedGame = gameState.SelectedGame
                .Where(g => g != null)
                .Do(gameMetadata.GetGameScreens);

            enrichedGame
                .Select(g => g is GameItem gameItem ? gameItem.Screens.LoadingScreenLocalPath : string.Empty)                
                .ToPropertyEx(this, x => x.LoadingScreenPath);

            enrichedGame
                .Select(g => g is GameItem gameItem ? gameItem.Screens.ScreenshotLocalPath : string.Empty)
                .ToPropertyEx(this, x => x.ScreenshotPath);
        }
    }
}
