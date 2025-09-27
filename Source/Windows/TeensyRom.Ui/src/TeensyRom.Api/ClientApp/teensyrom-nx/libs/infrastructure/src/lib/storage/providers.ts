import { STORAGE_SERVICE } from '@teensyrom-nx/domain';
import { StorageService } from './storage.service';

/**
 * Provider configuration for binding the storage service interface to its concrete implementation.
 * This enables dependency injection by interface while keeping the domain layer independent
 * of infrastructure implementations.
 */
export const STORAGE_SERVICE_PROVIDER = {
  provide: STORAGE_SERVICE,
  useClass: StorageService,
};