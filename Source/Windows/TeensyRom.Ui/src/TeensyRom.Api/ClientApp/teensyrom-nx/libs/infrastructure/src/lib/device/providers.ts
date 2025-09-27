import {
  DEVICE_SERVICE,
  DEVICE_LOGS_SERVICE,
  DEVICE_EVENTS_SERVICE,
  DEVICE_STORAGE_SERVICE,
} from '@teensyrom-nx/domain';
import { DeviceService } from './device.service';
import { DeviceLogsService } from './device-logs.service';
import { DeviceEventsService } from './device-events.service';
import { StorageService } from '../storage/storage.service';

export const DEVICE_SERVICE_PROVIDER = {
  provide: DEVICE_SERVICE,
  useClass: DeviceService,
};

export const DEVICE_LOGS_SERVICE_PROVIDER = {
  provide: DEVICE_LOGS_SERVICE,
  useClass: DeviceLogsService,
};

export const DEVICE_EVENTS_SERVICE_PROVIDER = {
  provide: DEVICE_EVENTS_SERVICE,
  useClass: DeviceEventsService,
};

export const DEVICE_STORAGE_SERVICE_PROVIDER = {
  provide: DEVICE_STORAGE_SERVICE,
  useClass: StorageService,
};

