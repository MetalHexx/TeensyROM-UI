using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public interface ILaunchHistory
    {
        void Add(ILaunchableItem fileItem);
        void Clear();
        ILaunchableItem? GetNext(params TeensyFileType[] types);
        ILaunchableItem? GetPrevious(params TeensyFileType[] types);
    }
}