using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Serial
{
    public interface IFwVersionChecker
    {
        (bool IsTeensyRom, bool? IsMinimal, bool? isVersionCompatible, Version? Version) GetAllVersionInfo(ISerialStateContext serial);
        (bool, Version?) VersionCheck(string response);
    }
}