using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Ui.Controls.DirectoryTree;

namespace TeensyRom.Ui.Features.Music.State
{
    public interface IMusicTreeState : IDirectoryTreeState { }
    public sealed class MusicTreeState : DirectoryTreeState, IMusicTreeState { }
}
