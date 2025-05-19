import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { Configuration as ApiClientConfig } from '../api-client/configuration';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes),
    provideHttpClient(),
    {
      provide: ApiClientConfig,
      useValue: new ApiClientConfig({
        basePath: 'http://localhost:5168'
      })
    }
  ]
};
