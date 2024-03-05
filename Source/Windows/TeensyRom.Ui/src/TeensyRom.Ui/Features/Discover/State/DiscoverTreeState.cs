using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Ui.Controls.DirectoryTree;

namespace TeensyRom.Ui.Features.Discover.State
{
    public interface IDiscoveryTreeState : IDirectoryTreeState { }
    public sealed class DiscoverTreeState : DirectoryTreeState, IDiscoveryTreeState { }
}
