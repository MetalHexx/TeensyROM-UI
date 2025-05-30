export interface DeviceStorage {
  deviceId: string;
  type: number;
  available: boolean;
}

export interface Device {
  deviceId: string;
  comPort: string;
  name: string;
  fwVersion: string;
  isCompatible: boolean;
  sdStorage: DeviceStorage;
  usbStorage: DeviceStorage;
}

export interface AllDevices {
  availableCarts: Device[];
  connectedCarts: Device[];
}
