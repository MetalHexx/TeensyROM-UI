using TeensyRom.Core.Music.Sid;

namespace TeensyRom.Core.Music.Hvsc
{
    public interface IHvscDatabase
    {
        SidRecord? GetRecord(string fileId);
    }
}