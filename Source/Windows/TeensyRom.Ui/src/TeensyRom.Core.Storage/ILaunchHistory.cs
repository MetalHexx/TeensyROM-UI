using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Storage
{
    public interface ILaunchHistory
    {
        void Add(LaunchableItem fileItem);
        void Clear();
        void ClearForwardHistory();
        LaunchableItem? GetNext(params TeensyFileType[] types);
        LaunchableItem? GetPrevious(params TeensyFileType[] types);
        bool CurrentIsNew { get; }
        void Remove(LaunchableItem fileItem);
    }
}