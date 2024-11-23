using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Features.Discover.State.Player
{
    public record LaunchedFileResult(ILaunchableItem File, bool Random);
}