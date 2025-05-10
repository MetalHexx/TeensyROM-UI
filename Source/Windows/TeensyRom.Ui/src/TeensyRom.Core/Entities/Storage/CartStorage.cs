namespace TeensyRom.Core.Entities.Storage
{
    public class CartStorage
    {
        public string DeviceId { get; set; } = string.Empty;
        public TeensyStorageType Type { get; set; }
        public bool Available { get; set; }
        public CartStorage() { }
        public CartStorage(TeensyStorageType storageType, bool available)
        {
            Type = storageType;
            Available = available;
        }

    }
}
