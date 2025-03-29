using System;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Discover.State.Player;
using TeensyRom.Core.Storage.Services;
using System.Reactive.Linq;
using System.Collections.Generic;
using TeensyRom.Ui.Features.Discover.State;
using TeensyRom.Core.Logging;
using System.Linq;

namespace TeensyRom.Ui.Services.Process
{
    public interface ICopyFileProcess
    {
        Task CopyFiles(IEnumerable<CopyFileItem> files);
    }
    public class CopyFileProcess(
        IPlayerContext player,
        ICachedStorageService storage,
        IAlertService alert) : ICopyFileProcess
    {
        public async Task CopyFiles(IEnumerable<CopyFileItem> files)
        {
            var currentFile = await player.LaunchedFile.FirstAsync();
            var playState = await player.PlayingState.FirstAsync();

            if (currentFile?.File is HexItem && playState is PlayState.Playing)
            {
                alert.Publish("Hex file is running, cannot add file to playlist");
                return;
            }

            if (currentFile?.File is GameItem && playState is PlayState.Playing)
            {
                await player.StopFile();
                await Task.Delay(500);
                alert.Publish("Game will restart momentarily.");
            }

            await storage.CopyFiles(files.ToList());

            if (currentFile?.File is GameItem game)
            {
                await Task.Delay(3000);
                await player.PlayFile(game);
            }
        }
    }
}