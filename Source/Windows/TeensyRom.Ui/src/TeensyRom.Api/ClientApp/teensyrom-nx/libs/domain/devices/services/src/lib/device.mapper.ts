import { CartDto, CartStorageDto, FindDevicesResponse } from '@teensyrom-nx/data-access/api-client';
import { AllDevices, Device, DeviceStorage } from './device.models';

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

  static toAllDevices(findDevicesResponse: FindDevicesResponse): AllDevices {
    return {
      availableCarts: this.toDeviceList(findDevicesResponse.availableCarts || []),
      connectedCarts: this.toDeviceList(findDevicesResponse.connectedCarts || []),
    };
  }
}
