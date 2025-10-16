namespace TeensyRom.Core.Music.DeepSid;

public interface IDeepSidDatabase
{
    DeepSidRecord? SearchByPath(string hvscPath);
}
