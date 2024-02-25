using System.Collections.Generic;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.PlayToolbar;
using TeensyRom.Ui.Features.NavigationHost;

namespace TeensyRom.Ui.Features.Common.Config
{
    public interface IExplorerViewConfig 
    {
        TeensyLibraryType LibraryType { get; set; }
        NavigationLocation NavigationLocation { get; set; }
        List<TeensyFileType> FileTypes { get; set; }
        PlayToggleOption PlayToggleOption { get; set; }
    }
}
