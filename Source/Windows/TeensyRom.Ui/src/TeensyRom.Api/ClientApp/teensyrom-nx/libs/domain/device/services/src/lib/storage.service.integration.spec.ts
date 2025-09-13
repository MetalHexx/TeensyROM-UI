import { describe, it, expect, beforeAll } from 'vitest';
import { StorageService } from './storage.service';
import {
  FilesApiService,
  Configuration,
  TeensyStorageType,
} from '@teensyrom-nx/data-access/api-client';
import { firstValueFrom } from 'rxjs';

describe('StorageService Integration Tests', () => {
  let storageService: StorageService;

  beforeAll(() => {
    // Create Configuration for the typescript-fetch client
    const config = new Configuration({
      basePath: 'http://localhost:5168',
      fetchApi: fetch, // Use standard fetch API
    });
    const filesApiService = new FilesApiService(config);
    storageService = new StorageService(filesApiService);
  });

  it('should return 404 error when index is called with a bad device ID', async () => {
    const badDeviceId = 'FAK2ZAJI';
    const storageType = 'SD' as TeensyStorageType;
    let error: Error | null = null;

    try {
      await firstValueFrom(storageService.index(badDeviceId, storageType));
    } catch (err) {
      error = err as Error;
    }

    expect(error).toBeDefined();
    expect(error?.message).toContain('Response returned an error code');
  });
});
