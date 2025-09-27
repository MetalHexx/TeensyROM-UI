import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { appRoutes } from './app.routes';
import { AppBootstrapService } from '@teensyrom-nx/app/bootstrap';
import {
  DEVICE_SERVICE_PROVIDER,
  DEVICE_LOGS_SERVICE_PROVIDER,
  DEVICE_EVENTS_SERVICE_PROVIDER,
  DEVICE_STORAGE_SERVICE_PROVIDER,
  STORAGE_SERVICE_PROVIDER,
  DEVICES_API_CLIENT_PROVIDER,
  FILES_API_CLIENT_PROVIDER,
} from '@teensyrom-nx/infrastructure';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes),
    provideHttpClient(),
    provideAnimations(),
    // API Client providers (infrastructure layer)
    DEVICES_API_CLIENT_PROVIDER,
    FILES_API_CLIENT_PROVIDER,
    // Domain service providers (infrastructure implementations)
    DEVICE_SERVICE_PROVIDER,
    DEVICE_LOGS_SERVICE_PROVIDER,
    DEVICE_EVENTS_SERVICE_PROVIDER,
    DEVICE_STORAGE_SERVICE_PROVIDER,
    STORAGE_SERVICE_PROVIDER,
    AppBootstrapService,
  ],
};
