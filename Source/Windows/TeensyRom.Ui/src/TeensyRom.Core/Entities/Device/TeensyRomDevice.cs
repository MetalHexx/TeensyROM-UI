using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Entities.Device
{
    public record TeensyRomDevice(Cart Cart, ISerialStateContext SerialState, IStorageService SdStorage, IStorageService UsbStorage)
    {
        public IStorageService? GetStorage(TeensyStorageType storageType)
        {
            if (storageType is TeensyStorageType.SD) 
            {
                if (!Cart.SdStorage.Available) return null;

                return SdStorage;
            }
            if (storageType is TeensyStorageType.USB)
            {
                if (!Cart.UsbStorage.Available) return null;

                return UsbStorage;
            }
            return null;
        }
    }
}
