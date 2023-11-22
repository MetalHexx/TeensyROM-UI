using System;
using TeensyRom.Ui.Features.Common.Models;

namespace TeensyRom.Ui.Features.Common.Models
{
    public class SongItemVm: StorageItem
    {
        public string ArtistName { get; set; } = string.Empty;
        public TimeSpan SongLength { get; set; } = TimeSpan.FromMinutes(3);
    }
}
