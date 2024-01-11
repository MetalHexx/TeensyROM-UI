using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public interface ILaunchHistory
    {
        void Add(FileItem fileItem);
        void Clear();
        FileItem? GetNext(params TeensyFileType[] types);
        FileItem? GetPrevious(params TeensyFileType[] types);
    }
}