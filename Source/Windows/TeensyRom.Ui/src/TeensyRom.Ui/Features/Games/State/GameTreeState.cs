using TeensyRom.Ui.Controls.DirectoryTree;

namespace TeensyRom.Ui.Features.Games.State.Directory
{
    public interface IGameDirectoryTreeState : IDirectoryTreeState { }
    public sealed class GameTreeState : DirectoryTreeState, IGameDirectoryTreeState { }
}
