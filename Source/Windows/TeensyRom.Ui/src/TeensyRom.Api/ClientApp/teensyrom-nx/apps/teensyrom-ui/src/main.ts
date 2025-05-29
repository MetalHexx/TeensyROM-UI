import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { runInInjectionContext, inject } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { AppBootstrapService } from '@teensyrom-nx/app-bootstrap';

async function bootstrap() {
  console.log('Starting application bootstrap...');

  // Create a temporary application instance to get access to DI
  const app = await bootstrapApplication(AppComponent, {
    ...appConfig,
    providers: [...appConfig.providers, provideHttpClient()],
  });

  // Run bootstrap initialization in the injection context
  await runInInjectionContext(app.injector, async () => {
    try {
      console.log('Initializing bootstrap service...');
      const bootstrapService = inject(AppBootstrapService);
      await bootstrapService.init();
      console.log('Bootstrap service initialized successfully');
    } catch (error) {
      console.error('Failed to initialize application:', error);
      if (error instanceof Error) {
        console.error('Error message:', error.message);
      }
    }
  });
}

// Bootstrap the application
console.log('About to bootstrap application...');
bootstrap().catch((err) => console.error('Bootstrap failed:', err));
