import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { appRoutes } from './app.routes';
import { AppBootstrapService } from '@teensyrom-nx/app-bootstrap';
import { DeviceService } from '@teensyrom-nx/device-services';
import { DevicesApiService, Configuration } from '@teensyrom-nx/api-client';
import { HttpClient } from '@angular/common/http';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes),
    provideHttpClient(),
    {
      provide: DevicesApiService,
      useFactory: (httpClient: HttpClient) => {
        const config = new Configuration({ basePath: 'http://localhost:5168' });
        return new DevicesApiService(httpClient, config.basePath || '', config);
      },
      deps: [HttpClient],
    },
    DeviceService,
    AppBootstrapService,
  ],
};
