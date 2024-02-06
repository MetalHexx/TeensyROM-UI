using TeensyRom.Ui.Controls.DirectoryTree;

namespace TeensyRom.Ui.Features.Games.State.NewState
{
    public interface IGameDirectoryTreeState : IDirectoryTreeState { }
    public sealed class GameDirectoryTreeState : DirectoryTreeState, IGameDirectoryTreeState { }
}
