export * from './devices.api.service';
export * from './files.api.service';
import { DevicesApiService } from './devices.api.service';
import { FilesApiService } from './files.api.service';
export const APIS = [DevicesApiService, FilesApiService];
