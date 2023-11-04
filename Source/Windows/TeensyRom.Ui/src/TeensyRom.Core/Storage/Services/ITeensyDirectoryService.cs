using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public interface ITeensyDirectoryService
    {
        Task<DirectoryContent?> GetDirectoryContentAsync(string path);
    }
}
