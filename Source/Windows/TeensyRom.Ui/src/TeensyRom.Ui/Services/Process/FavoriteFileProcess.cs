using System;
using System.Threading.Tasks;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Ui.Features.Discover.State.Player;
using System.Reactive.Linq;
using TeensyRom.Ui.Features.Discover.State;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Storage;

namespace TeensyRom.Ui.Services.Process
{
    public interface IFavoriteFileProcess
    {
        Task SaveFavorite(ILaunchableItem file);
        Task RemoveFavorite(ILaunchableItem file);
    }

    public class FavoriteFileProcess(
        IPlayerContext player,
        ICachedStorageService storage,
        IAlertService alert) : IFavoriteFileProcess
    {
        public async Task SaveFavorite(ILaunchableItem file)
        {
            await ProcessFavorite(file, async () => await storage.SaveFavorite(file) ?? file);
        }

        public async Task RemoveFavorite(ILaunchableItem file)
        {
            await ProcessFavorite(file, async () => { await storage.RemoveFavorite(file); return file; });
        }

        private async Task ProcessFavorite(ILaunchableItem file, Func<Task<ILaunchableItem>> storageAction)
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

            var isGame = currentFile.File is GameItem;

            if (isGame)
            {
                alert.Publish("Your game will re-launch to allow favorite to be tagged.");
                await player.StopFile();
            }

            await storageAction();

            if (isGame)
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