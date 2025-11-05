import { InjectionToken } from '@angular/core';
import { IStorageService } from './storage.contract';

/**
 * Injection token for device-specific storage service.
 * Uses the same IStorageService interface but represents a different implementation
 * or configuration for device-specific storage operations.
 */
export const DEVICE_STORAGE_SERVICE = new InjectionToken<IStorageService>('DEVICE_STORAGE_SERVICE');
