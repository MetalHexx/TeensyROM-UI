using System;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Discover.State.Player;
using TeensyRom.Core.Storage.Services;
using System.Reactive.Linq;
using TeensyRom.Ui.Features.Discover.State;
using TeensyRom.Core.Logging;

namespace TeensyRom.Ui.Services.Process
{
    public interface IFavoriteFileProcess
    {
        Task SaveFavorite(ILaunchableItem file);
    }

    public class FavoriteFileProcess(
        IPlayerContext player,
        ICachedStorageService storage,
        IAlertService alert) : IFavoriteFileProcess
    {
        public async Task SaveFavorite(ILaunchableItem file)
        {
            var currentFile = await player.LaunchedFile.FirstAsync();
            var playState = await player.PlayingState.FirstAsync();
            var playerState = await player.CurrentState.FirstAsync();
            var currentPath = await player.CurrentPath.FirstAsync();

            if (currentFile.File is HexItem && playState is PlayState.Playing)
            {
                alert.Publish("Hex file is running, cannot tag favorite.");
                return;
            }
            if (currentFile.File is GameItem)
            {
                alert.Publish("Your game will re-launch to allow favorite to be tagged.");
                await player.StopFile();
            }
            var favFile = await storage.SaveFavorite(file);

            if (currentFile.File is GameItem)
            {
                await player.PlayFile(currentFile.File);
            }
            if (playerState is not SearchState)
            {
                await player.LoadDirectory(currentPath, file.Path);
            }
        }
    }
}