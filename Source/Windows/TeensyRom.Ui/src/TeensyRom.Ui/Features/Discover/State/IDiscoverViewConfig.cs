using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Common.Config;
using TeensyRom.Ui.Features.NavigationHost;

namespace TeensyRom.Ui.Features.Discover.State
{
    public interface IDiscoverViewConfig : IExplorerViewConfig { }

    public class DiscoverViewConfig : IDiscoverViewConfig
    {
        public List<TeensyFileType> FileTypes { get; set; } = [TeensyFileType.Sid, TeensyFileType.Crt, TeensyFileType.Prg];
        public NavigationLocation NavigationLocation { get; set; } = NavigationLocation.Discover;
        public TeensyFilterType FilterType { get; set; } = TeensyFilterType.All;
    }
}
