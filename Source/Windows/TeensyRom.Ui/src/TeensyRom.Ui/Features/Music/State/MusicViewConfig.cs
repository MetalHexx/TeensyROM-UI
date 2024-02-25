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

namespace TeensyRom.Ui.Features.Music.State
{
    public interface IMusicViewConfig : IExplorerViewConfig { }

    public class MusicViewConfig : IMusicViewConfig
    {
        public List<TeensyFileType> FileTypes { get; set; } = [TeensyFileType.Sid];
        public PlayToggleOption PlayToggleOption { get; set; } = PlayToggleOption.Pause;
        public NavigationLocation NavigationLocation { get; set; } = NavigationLocation.Music;
        public TeensyLibraryType LibraryType { get; set; } = TeensyLibraryType.Music;
    }
}
