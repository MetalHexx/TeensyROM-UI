using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Common.Abstractions;
using TeensyRom.Core.Music.Sid;

namespace TeensyRom.Core.Music.Images
{

    public class MusicUnpackService :  UnpackService, IUnpackAssetService
    {
        public MusicUnpackService() : base(SidConstants.Musician_Image_Local_Path, "Composers.zip") { }        
    }
}
