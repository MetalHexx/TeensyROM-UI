import { describe, it, expect, beforeAll } from 'vitest';
import { StorageService } from '../storage/storage.service';
import { FilesApiService, Configuration } from '@teensyrom-nx/data-access/api-client';
import { StorageType } from '@teensyrom-nx/domain';
import { firstValueFrom } from 'rxjs';

// Gate integration tests behind env variable to avoid external dependency by default
const run = process.env.RUN_INTEGRATION === 'true' ? describe : describe.skip;

run('StorageService Integration Tests', () => {
  let storageService: StorageService;

  beforeAll(() => {
    const config = new Configuration({
      basePath: 'http://localhost:5168',
      fetchApi: fetch,
    });
    const filesApiService = new FilesApiService(config);
    storageService = new StorageService(filesApiService);
  });

  it('should return 404 error when index is called with a bad device ID', async () => {
    const badDeviceId = 'FAK2ZAJI';
    const storageType = StorageType.Sd;
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
