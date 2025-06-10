import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { appRoutes } from './app.routes';
import { AppBootstrapService } from '@teensyrom-nx/app/bootstrap';
import { DeviceService } from '@teensyrom-nx/domain/device/services';
import {
  DevicesApiService,
  Configuration,
  FilesApiService,
} from '@teensyrom-nx/data-access/api-client';
import { HttpClient } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes),
    provideHttpClient(),
    provideAnimations(),
    {
      provide: DevicesApiService,
      useFactory: (httpClient: HttpClient) => {
        const config = new Configuration({ basePath: 'http://localhost:5168' });
        return new DevicesApiService(httpClient, config.basePath || '', config);
      },
      deps: [HttpClient],
    },
    {
      provide: FilesApiService,
      useFactory: (httpClient: HttpClient) => {
        const config = new Configuration({ basePath: 'http://localhost:5168' });
        return new FilesApiService(httpClient, config.basePath || '', config);
      },
      deps: [HttpClient],
    },
    DeviceService,
    AppBootstrapService,
  ],
};
