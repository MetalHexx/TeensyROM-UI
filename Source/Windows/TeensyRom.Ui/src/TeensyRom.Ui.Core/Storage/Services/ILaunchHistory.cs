using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Storage.Services
{
    public interface ILaunchHistory
    {
        void Add(ILaunchableItem fileItem);
        void Clear();
        void ClearForwardHistory();
        ILaunchableItem? GetNext(params TeensyFileType[] types);
        ILaunchableItem? GetPrevious(params TeensyFileType[] types);
        bool CurrentIsNew { get; }
        void Remove(ILaunchableItem fileItem);
    }
}