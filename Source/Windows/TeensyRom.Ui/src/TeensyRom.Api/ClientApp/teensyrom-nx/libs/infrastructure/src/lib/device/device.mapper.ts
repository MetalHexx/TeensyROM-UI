import { CartDto, CartStorageDto } from '@teensyrom-nx/data-access/api-client';
import { Device, DeviceStorage, DeviceState, StorageType } from '@teensyrom-nx/domain';
import { DeviceState as ApiDeviceState } from '@teensyrom-nx/data-access/api-client';
import { TeensyStorageType } from '@teensyrom-nx/data-access/api-client';

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
      deviceState: this.mapDeviceState(cartDto.deviceState),
      sdStorage: this.toDeviceStorage(cartDto.sdStorage),
      usbStorage: this.toDeviceStorage(cartDto.usbStorage),
    };
  }

  static toDeviceStorage(storageDto: CartStorageDto): DeviceStorage {
    return {
      deviceId: storageDto.deviceId,
      type: this.mapStorageType(storageDto.type),
      available: storageDto.available,
    };
  }

  private static mapDeviceState(apiState: ApiDeviceState): DeviceState {
    // Both enums have the same values, so we can safely cast
    return apiState as unknown as DeviceState;
  }

  private static mapStorageType(apiType: TeensyStorageType): StorageType {
    // Both enums have the same values, so we can safely cast
    return apiType as unknown as StorageType;
  }

  static toDeviceList(cartDtos: CartDto[]): Device[] {
    return cartDtos.map((cart) => this.toDevice(cart));
  }
}

