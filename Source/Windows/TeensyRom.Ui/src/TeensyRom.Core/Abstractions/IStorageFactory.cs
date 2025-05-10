using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Storage
{
    public interface IStorageFactory
    {
        IStorageService Create(CartStorage cartStorage);
    }
}
