import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { appRoutes } from './app.routes';
import { AppBootstrapService } from '@teensyrom-nx/app/bootstrap';
import { DeviceService } from '@teensyrom-nx/domain/device/services';
import { STORAGE_SERVICE_PROVIDER } from '@teensyrom-nx/domain/storage/services';
import {
  DevicesApiService,
  Configuration,
  FilesApiService,
} from '@teensyrom-nx/data-access/api-client';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes),
    provideHttpClient(),
    provideAnimations(),
    {
      provide: DevicesApiService,
      useFactory: () => {
        const config = new Configuration({ basePath: 'http://localhost:5168' });
        return new DevicesApiService(config);
      },
    },
    {
      provide: FilesApiService,
      useFactory: () => {
        const config = new Configuration({ basePath: 'http://localhost:5168' });
        return new FilesApiService(config);
      },
    },
    DeviceService,
    STORAGE_SERVICE_PROVIDER,
    AppBootstrapService,
  ],
};
