using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Features.Common.Config;
using TeensyRom.Ui.Features.NavigationHost;

namespace TeensyRom.Ui.Features.Games.State
{
    public interface IGameViewConfig : IExplorerViewConfig { }

    public class GameViewConfig : IGameViewConfig
    {
        public List<TeensyFileType> FileTypes { get; set; } = [TeensyFileType.Crt, TeensyFileType.Prg];
        public NavigationLocation NavigationLocation { get; set; } = NavigationLocation.Games;
        public TeensyLibraryType LibraryType { get; set; } = TeensyLibraryType.Programs;
    }
}
