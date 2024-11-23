namespace TeensyRom.Ui.Core.Music.Sid
{
    public class SidRecord
    {
        public string Filename { get; set; } = string.Empty;
        public string Filepath { get; set; } = string.Empty;
        //public string Format { get; set; } = string.Empty;
        //public string Version { get; set; } = string.Empty;
        //public string DataOffset { get; set; } = string.Empty;
        //public string Md5 { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Released { get; set; } = string.Empty;
        //public int Songs { get; set; }
        public int StartSong { get; set; }
        public string SongLength { get; set; } = string.Empty;
        public TimeSpan SongLengthSpan { get; set; } = TimeSpan.FromMinutes(3);
        public List<TimeSpan> SubTuneSongLengths = [];
        public int SizeInBytes { get; set; }
        //public string InitAddr { get; set; } = string.Empty;
        //public string PlayAddr { get; set; } = string.Empty;
        //public string LoadRange { get; set; } = string.Empty;
        //public string Speed { get; set; } = string.Empty;
        //public string MusPlayer { get; set; } = string.Empty;
        //public string PlaySidC64Basic { get; set; } = string.Empty;
        public string Clock { get; set; } = string.Empty;
        public string SidModel { get; set; } = string.Empty;
        //public string StartPage { get; set; } = string.Empty;
        //public string PageLength { get; set; } = string.Empty;
        //public string SecondSidAddr { get; set; } = string.Empty;
        //public string ThirdSidAddr { get; set; } = string.Empty;
        public string Stil { get; set; } = string.Empty;
        public string StilEntry { get; set; } = string.Empty;
    }

}
