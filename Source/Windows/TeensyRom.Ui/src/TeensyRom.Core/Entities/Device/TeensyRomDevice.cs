using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Entities.Device
{
    public record TeensyRomDevice(Cart Cart, ISerialStateContext SerialState, IStorageService SdStorage, IStorageService UsbStorage);
}
