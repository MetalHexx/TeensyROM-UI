using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Serial
{
    public class CartStorage 
    {
        public TeensyStorageType Type { get; set; }
        public bool Available { get; set; }
        public CartStorage() {}
        public CartStorage(TeensyStorageType storageType, bool available)
        {
            Type = storageType;
            Available = available;
        }

    }
}
