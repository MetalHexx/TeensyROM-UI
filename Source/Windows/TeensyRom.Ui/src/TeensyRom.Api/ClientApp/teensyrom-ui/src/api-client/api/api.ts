export * from './devices.service';
import { DevicesService } from './devices.service';
export * from './devices.serviceInterface';
export * from './files.service';
import { FilesService } from './files.service';
export * from './files.serviceInterface';
export const APIS = [DevicesService, FilesService];
