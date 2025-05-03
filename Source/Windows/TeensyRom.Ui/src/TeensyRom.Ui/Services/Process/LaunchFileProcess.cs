using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Ui.Features.Discover.State.Player;

namespace TeensyRom.Ui.Services.Process
{
    public interface ILaunchFileProcess
    {
        void LaunchFile(ILaunchableItem file);
    }
    public class LaunchFileProcess(IPlayerContext player) : ILaunchFileProcess
    {
        public void LaunchFile(ILaunchableItem file) 
        {
            player.PlayFile(file);
            player.LoadDirectory(file.Path.GetUnixParentPath(), file.Path);
        }
    }
}