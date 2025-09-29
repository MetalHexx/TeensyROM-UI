import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { appRoutes } from './app.routes';
import { AppBootstrapService } from '@teensyrom-nx/app/bootstrap';
import { PLAYER_CONTEXT_PROVIDER } from '@teensyrom-nx/application';
import {
  DEVICE_SERVICE_PROVIDER,
  DEVICE_LOGS_SERVICE_PROVIDER,
  DEVICE_EVENTS_SERVICE_PROVIDER,
  DEVICE_STORAGE_SERVICE_PROVIDER,
  STORAGE_SERVICE_PROVIDER,
  DEVICES_API_CLIENT_PROVIDER,
  FILES_API_CLIENT_PROVIDER,
  PLAYER_SERVICE_PROVIDER,
  PLAYER_API_CLIENT_PROVIDER,
} from '@teensyrom-nx/infrastructure';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(appRoutes),
    provideHttpClient(),
    provideAnimations(),
    AppBootstrapService,
    PLAYER_CONTEXT_PROVIDER,
    // Infrastructure providers (composition root)
    DEVICE_SERVICE_PROVIDER,
    DEVICE_LOGS_SERVICE_PROVIDER,
    DEVICE_EVENTS_SERVICE_PROVIDER,
    DEVICE_STORAGE_SERVICE_PROVIDER,
    STORAGE_SERVICE_PROVIDER,
    DEVICES_API_CLIENT_PROVIDER,
    FILES_API_CLIENT_PROVIDER,
    PLAYER_SERVICE_PROVIDER,
    PLAYER_API_CLIENT_PROVIDER,
  ],
};
