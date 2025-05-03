using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Ui.Features.Discover.State.Player
{
    public record LaunchedFileResult(ILaunchableItem File, bool Random);
}