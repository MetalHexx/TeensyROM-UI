using TeensyRom.Core.Entities.Device;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Api.Endpoints.Common
{
    public class CartDto
    {
        public string? DeviceId { get; set; }
        public string ComPort { get; set; } = string.Empty;
        public string Name { get; set; } = "Unnamed";
        public string FwVersion { get; set; } = string.Empty;
        public bool IsCompatible { get; set; }
        public CartStorageDto SdStorage { get; set; } = null!;
        public CartStorageDto UsbStorage { get; set; } = null!;

        public static CartDto FromCart(Cart cart)
        {
            return new CartDto
            {
                DeviceId = cart.DeviceId,
                ComPort = cart.ComPort,
                Name = cart.Name,
                FwVersion = cart.FwVersion,
                IsCompatible = cart.IsCompatible,
                SdStorage = CartStorageDto.FromStorage(cart.SdStorage),
                UsbStorage = CartStorageDto.FromStorage(cart.UsbStorage)
            };
        }
    }

    public class CartStorageDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public TeensyStorageType Type { get; set; }
        public bool Available { get; set; }

        public static CartStorageDto FromStorage(CartStorage storage)
        {
            return new ()
            {
                DeviceId = storage.DeviceId,
                Type = storage.Type,
                Available = storage.Available
            };
        }
    }
}
