using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Features.Common.Config;

namespace TeensyRom.Ui.Features.Games.State
{
    public interface IGameViewConfig : IExplorerViewConfig { }

    public class GameViewConfig : IGameViewConfig
    {
        public List<TeensyFileType> FileTypes { get; set; } = [TeensyFileType.Crt, TeensyFileType.Prg];
        public PlayToggleOption PlayToggleOption { get; set; } = PlayToggleOption.Stop;
    }
}
