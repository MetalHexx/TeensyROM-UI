import { Injectable } from '@angular/core';
import { FilesApiService, TeensyStorageType } from '@teensyrom-nx/data-access/api-client';
import { from } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class StorageService {
  constructor(private readonly filesApiService: FilesApiService) {}

  index(deviceId: string, storageType: TeensyStorageType, startingPath?: string) {
    return from(this.filesApiService.index({ deviceId, storageType, startingPath }));
  }

  indexAll() {
    return from(this.filesApiService.indexAll({}));
  }
}
