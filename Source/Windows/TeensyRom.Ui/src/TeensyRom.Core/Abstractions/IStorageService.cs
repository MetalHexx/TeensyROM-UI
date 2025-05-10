namespace TeensyRom.Core.Abstractions
{
    public interface IStorageService
    {
        Task CacheAll();
        Task Cache(string path);
        public void ClearCache();
        public void ClearCache(string path);
    }
}
