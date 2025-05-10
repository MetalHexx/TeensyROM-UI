using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Entities.Device
{
    public class Cart
    {
        public string? DeviceId { get; set; }
        public string ComPort { get; set; } = string.Empty;
        public string Name { get; set; } = "Unnamed";
        public string FwVersion { get; set; } = string.Empty;
        public bool IsCompatible { get; set; }
        public CartStorage SdStorage { get; set; } = new(TeensyStorageType.SD, available: false);
        public CartStorage UsbStorage { get; set; } = new(TeensyStorageType.USB, available: false);
    }
}
