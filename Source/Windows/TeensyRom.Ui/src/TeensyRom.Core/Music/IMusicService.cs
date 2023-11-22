namespace TeensyRom.Core.Music
{
    public interface IMusicService
    {
        SidRecord? Find(string filePath);
    }
}