using System;
using System.Numerics;
using System.Text.Json.Serialization;
using TeensyRom.Ui.Core.Common;

namespace TeensyRom.Ui.Core.Storage.Entities
{
    public class SongItem : FileItem, ILaunchableItem, IViewableItem
    {   
        public TimeSpan PlayLength { get; set; } = TimeSpan.FromMinutes(3);
        public List<TimeSpan> SubtuneLengths { get; set; } = [];
        public int StartSubtuneNum { get; set; }
        public List<ViewableItemImage> Images { get; init; } = [];        

        public SongItem()
        {
            Creator = "Unknown";
            ReleaseInfo = "Unknown";
            Description = "About SID Files:\r\rSID files are programs used to control the Commodore 64's Sound Interface Device (SID) to make music for games and standalone compositions. \r\rThe SID is a unique and forward thinking 3-voice polyphonic hybrid synthesizer that combines digital oscillators and a multimode analog filter. The .sid files contain instructions to directly control the SID chip by orchestrating it's large selection of available parameters in realtime. \r\rA significant portion of SID music, especially in the early years, was programmed with extreme precision using assembly language and BASIC. Trackers and dedicated music software became more prevalent later, offering a more accessible interface for SID music creation.";
        }

        public override SongItem Clone() => new()
        {
            Name = Name,
            Path = Path,
            Size = Size,
            IsFavorite = IsFavorite,
            Title = Title,
            Creator = Creator,
            ReleaseInfo = ReleaseInfo,
            Description = Description,
            ShareUrl = ShareUrl,
            MetadataSource = MetadataSource,
            Meta1 = Meta1,
            Meta2 = Meta2,
            MetadataSourcePath = MetadataSourcePath,
            PlayLength = PlayLength,
            Images = Images.Select(x => x.Clone()).ToList()
        };
    }
}
