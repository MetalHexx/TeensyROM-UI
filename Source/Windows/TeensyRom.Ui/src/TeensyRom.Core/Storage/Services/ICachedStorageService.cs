using TeensyRom.Core.Music;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public interface ICachedStorageService<T> : IDisposable where T : FileItem
    {
        void ClearCache();
        void ClearCache(string path);
        Task<FileDirectory?> GetDirectory(string path);
        Task<T?> SaveFavorite(T item);
    }
}