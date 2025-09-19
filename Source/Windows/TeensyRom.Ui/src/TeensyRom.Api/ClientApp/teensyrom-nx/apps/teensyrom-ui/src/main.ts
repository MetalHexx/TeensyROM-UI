import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { runInInjectionContext, inject } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { AppBootstrapService } from '@teensyrom-nx/app/bootstrap';
import { LogType, logInfo, logError } from '@teensyrom-nx/utils';

async function bootstrap() {
  logInfo(LogType.Start, 'Starting application bootstrap...');

  // Create a temporary application instance to get access to DI
  const app = await bootstrapApplication(AppComponent, {
    ...appConfig,
    providers: [...appConfig.providers, provideHttpClient()],
  });

  // Run bootstrap initialization in the injection context
  await runInInjectionContext(app.injector, async () => {
    try {
      logInfo(LogType.Start, 'Initializing bootstrap service...');
      const bootstrapService = inject(AppBootstrapService);
      await bootstrapService.init();
      logInfo(LogType.Success, 'Bootstrap service initialized successfully');
    } catch (error) {
      logError('Failed to initialize application:', error);
      if (error instanceof Error) {
        logError('Error message:', error);
      }
    }
  });
}

// Bootstrap the application
logInfo(LogType.Info, 'About to bootstrap application...');
bootstrap().catch((err) => logError('Bootstrap failed:', err));
