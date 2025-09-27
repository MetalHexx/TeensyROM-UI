import { CartDto, CartStorageDto } from '@teensyrom-nx/data-access/api-client';
import { Device, DeviceStorage } from '@teensyrom-nx/domain';

export class DeviceMapper {
  static toDevice(cartDto: CartDto): Device {
    if (!cartDto) {
      return {} as Device;
    }

    return {
      deviceId: cartDto.deviceId,
      comPort: cartDto.comPort,
      name: cartDto.name,
      fwVersion: cartDto.fwVersion,
      isCompatible: cartDto.isCompatible,
      isConnected: cartDto.isConnected,
      deviceState: cartDto.deviceState,
      sdStorage: this.toDeviceStorage(cartDto.sdStorage),
      usbStorage: this.toDeviceStorage(cartDto.usbStorage),
    };
  }

  static toDeviceStorage(storageDto: CartStorageDto): DeviceStorage {
    return {
      deviceId: storageDto.deviceId,
      type: storageDto.type,
      available: storageDto.available,
    };
  }

  static toDeviceList(cartDtos: CartDto[]): Device[] {
    return cartDtos.map((cart) => this.toDevice(cart));
  }
}

