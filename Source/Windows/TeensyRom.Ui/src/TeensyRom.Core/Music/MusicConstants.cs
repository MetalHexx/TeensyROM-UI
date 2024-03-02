using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Music
{
    public static class MusicConstants
    {
        public readonly static TimeSpan DefaultLength = TimeSpan.FromMinutes(3);
        public const string Hvsc = "HVSC";
        public const string Hvsc_Local_Path = @"Assets\Music\Metadata\SIDlist_79_UTF8.csv";
        public const string DeepSid = "DeepSID";
        public const string Musician_Image_Local_Path = @"Assets\Music\Images\Composers\";
        public const string Hvsc_Musician_Base_Remote_Path = @"/MUSICIANS/";
    }
}
