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
        Task SaveFavorite(LaunchableItem file);
        Task RemoveFavorite(LaunchableItem file);
    }

    public class FavoriteFileProcess(
        IPlayerContext player,
        ICachedStorageService storage,
        IAlertService alert) : IFavoriteFileProcess
    {
        public async Task SaveFavorite(LaunchableItem file)
        {
            await ProcessFavorite(file, async () => await storage.SaveFavorite(file) ?? file);
        }

        public async Task RemoveFavorite(LaunchableItem file)
        {
            await ProcessFavorite(file, async () => { await storage.RemoveFavorite(file); return file; });
        }

        private async Task ProcessFavorite(LaunchableItem file, Func<Task<LaunchableItem>> storageAction)
        {
            var currentFile = await player.LaunchedFile.FirstOrDefaultAsync();
            var playState = await player.PlayingState.FirstOrDefaultAsync();
            var playerState = await player.CurrentState.FirstOrDefaultAsync();
            var currentPath = await player.CurrentPath.FirstOrDefaultAsync();

            if (currentFile?.File is HexItem && playState is PlayState.Playing)
            {
                alert.Publish("Hex file is running, cannot tag favorite.");
                return;
            }

            var isGame = currentFile?.File is GameItem;

            if (isGame)
            {
                alert.Publish("Your game will re-launch to allow favorite to be tagged.");
                await player.StopFile();
            }

            await storageAction();

            if (isGame && currentFile is not null)
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