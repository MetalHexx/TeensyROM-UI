namespace TeensyRom.Core.Serial
{
    public interface IFwVersionChecker
    {
        (bool, Version?) VersionCheck(string response);
    }
}