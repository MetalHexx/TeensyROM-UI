import {
  DEVICE_SERVICE,
  DEVICE_LOGS_SERVICE,
  DEVICE_EVENTS_SERVICE,
  DEVICE_STORAGE_SERVICE,
  ALERT_SERVICE,
} from '@teensyrom-nx/domain';
import { DeviceService } from './device.service';
import { DeviceLogsService } from './device-logs.service';
import { DeviceEventsService } from './device-events.service';
import { StorageService } from '../storage/storage.service';
import {
  DevicesApiService,
  Configuration,
} from '@teensyrom-nx/data-access/api-client';

// API Client provider
export const DEVICES_API_CLIENT_PROVIDER = {
  provide: DevicesApiService,
  useFactory: () => {
    const config = new Configuration({ basePath: 'http://localhost:5168' });
    return new DevicesApiService(config);
  },
};

export const DEVICE_SERVICE_PROVIDER = {
  provide: DEVICE_SERVICE,
  useClass: DeviceService,
  deps: [DevicesApiService, ALERT_SERVICE],
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

