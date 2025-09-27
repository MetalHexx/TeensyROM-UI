import { STORAGE_SERVICE } from '@teensyrom-nx/domain';
import { StorageService } from './storage.service';
import {
  FilesApiService,
  Configuration,
} from '@teensyrom-nx/data-access/api-client';

// API Client provider for Files
export const FILES_API_CLIENT_PROVIDER = {
  provide: FilesApiService,
  useFactory: () => {
    const config = new Configuration({ basePath: 'http://localhost:5168' });
    return new FilesApiService(config);
  },
};

/**
 * Provider configuration for binding the storage service interface to its concrete implementation.
 * This enables dependency injection by interface while keeping the domain layer independent
 * of infrastructure implementations.
 */
export const STORAGE_SERVICE_PROVIDER = {
  provide: STORAGE_SERVICE,
  useClass: StorageService,
  deps: [FilesApiService],
};