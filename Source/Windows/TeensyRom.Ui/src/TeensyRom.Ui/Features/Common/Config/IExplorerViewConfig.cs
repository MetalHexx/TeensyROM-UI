using System.Collections.Generic;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Controls.PlayToolbar;

namespace TeensyRom.Ui.Features.Common.Config
{
    public interface IExplorerViewConfig 
    {
        List<TeensyFileType> FileTypes { get; set; }
        PlayToggleOption PlayToggleOption { get; set; }
    }
}
