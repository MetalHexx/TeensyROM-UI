using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Entities.Device
{
    public class TeensyRomDevice
    {
        public Cart Cart { get; private set; }
        public ISerialStateContext SerialState { get; private set; }
        public IStorageService SdStorage { get; private set; }
        public IStorageService UsbStorage { get; private set; }
        public bool IsConnected => SerialState.IsOpen;
        public string DeviceId => Cart.DeviceId;

        public TeensyRomDevice(Cart cart, ISerialStateContext serialState, IStorageService sdStorage, IStorageService usbStorage)
        {
            Cart = cart;
            SerialState = serialState;
            SdStorage = sdStorage;
            UsbStorage = usbStorage;
        }

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
