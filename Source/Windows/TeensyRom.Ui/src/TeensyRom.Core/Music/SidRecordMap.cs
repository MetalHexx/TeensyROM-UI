using CsvHelper.Configuration;

namespace TeensyRom.Core.Music
{
    public sealed class SidRecordMap : ClassMap<SidRecord>
    {
        public SidRecordMap()
        {
            Map(m => m.Filename).Name("FILENAME");
            Map(m => m.Format).Name("FORMAT");
            Map(m => m.Version).Name("VERSION");
            Map(m => m.DataOffset).Name("DATAOFFSET");
            Map(m => m.Md5).Name("MD5");
            Map(m => m.Title).Name("TITLE");
            Map(m => m.Author).Name("AUTHOR");
            Map(m => m.Released).Name("RELEASED");
            Map(m => m.Songs).Name("SONGS");
            Map(m => m.StartSong).Name("STARTSONG");
            Map(m => m.SongLength).Name("SONGLEN (per tune)");
            Map(m => m.SizeInBytes).Name("SIZE (in bytes)");
            Map(m => m.InitAddr).Name("INITADDR");
            Map(m => m.PlayAddr).Name("PLAYADDR");
            Map(m => m.LoadRange).Name("LOADRANGE");
            Map(m => m.Speed).Name("SPEED");
            Map(m => m.MusPlayer).Name("MUSPLAYER");
            Map(m => m.PlaySidC64Basic).Name("PLAYSID/C64BASIC");
            Map(m => m.Clock).Name("CLOCK");
            Map(m => m.SidModel).Name("SIDMODEL");
            Map(m => m.StartPage).Name("STARTPAGE");
            Map(m => m.PageLength).Name("PAGELEN");
            Map(m => m.SecondSidAddr).Name("SECONDSIDADDR");
            Map(m => m.ThirdSidAddr).Name("THIRSIDADDR");
            Map(m => m.Stil).Name("STIL");
            Map(m => m.StilEntry).Name("STILENTRY");
        }
    }

}
