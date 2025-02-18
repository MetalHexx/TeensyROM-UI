namespace TeensyRom.Core.Serial
{
    public interface IFwVersionChecker
    {
        bool VersionCheck(string response);
    }
}