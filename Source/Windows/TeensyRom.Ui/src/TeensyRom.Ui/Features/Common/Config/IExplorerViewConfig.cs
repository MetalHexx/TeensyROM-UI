using System.Collections.Generic;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Settings;

namespace TeensyRom.Ui.Features.Common.Config
{
    public interface IExplorerViewConfig 
    {
        TeensyFilterType FilterType { get; set; }
        NavigationLocation NavigationLocation { get; set; }
        List<TeensyFileType> FileTypes { get; set; }
    }
}
