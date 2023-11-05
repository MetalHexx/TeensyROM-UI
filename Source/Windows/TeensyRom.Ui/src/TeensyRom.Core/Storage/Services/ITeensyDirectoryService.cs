using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public interface ITeensyDirectoryService
    {
        DirectoryContent? GetDirectoryContent(string path, uint skip, uint take);
    }
}
