// Shared models
export * from './lib/models';

// Device domain exports
export * from './lib/device/contracts/device.contract';
export * from './lib/device/contracts/device-logs.contract';
export * from './lib/device/contracts/device-events.contract';

// Storage domain exports
export * from './lib/storage/contracts/storage.contract';

// Device storage token (uses same interface as main storage service)
import { InjectionToken } from '@angular/core';
import { IStorageService } from './lib/storage/contracts/storage.contract';
export const DEVICE_STORAGE_SERVICE = new InjectionToken<IStorageService>('DEVICE_STORAGE_SERVICE');

// Future: Additional domain exports will go here
// export * from './lib/player/models/player.models';
// export * from './lib/user/contracts/user.contract';