import { describe, it, expect, beforeAll } from 'vitest';
import { StorageService } from './storage.service';
import {
  FilesApiService,
  Configuration,
  TeensyStorageType,
} from '@teensyrom-nx/data-access/api-client';
import { HttpClient, HttpXhrBackend, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

describe('StorageService Integration Tests', () => {
  let storageService: StorageService;

  beforeAll(() => {
    const httpHandler = new HttpXhrBackend({ build: () => new XMLHttpRequest() });
    const httpClient = new HttpClient(httpHandler);
    const config = new Configuration({ basePath: 'http://localhost:5168' });
    const filesApiService = new FilesApiService(httpClient, config.basePath || '', config);
    storageService = new StorageService(filesApiService);
  });

  it('should return 404 error when index is called with a bad device ID', async () => {
    const badDeviceId = 'FAK2ZAJI';
    const storageType = 'SD' as TeensyStorageType;
    let error: HttpErrorResponse | null = null;

    try {
      await firstValueFrom(storageService.index(badDeviceId, storageType));
    } catch (err) {
      if (err instanceof HttpErrorResponse) {
        error = err;
      }
    }

    expect(error).toBeInstanceOf(HttpErrorResponse);
    expect(error?.status).toBe(404);
    expect(error?.statusText).toBe('Not Found');
  });
});
