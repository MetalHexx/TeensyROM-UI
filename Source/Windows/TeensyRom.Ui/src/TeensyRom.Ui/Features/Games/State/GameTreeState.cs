using TeensyRom.Ui.Controls.DirectoryTree;

namespace TeensyRom.Ui.Features.Games.State.Directory
{
    public interface IGameTreeState : IDirectoryTreeState { }
    public sealed class GameTreeState : DirectoryTreeState, IGameTreeState { }
}
