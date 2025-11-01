import { nxE2EPreset } from '@nx/cypress/plugins/cypress-preset';
import { defineConfig } from 'cypress';

export default defineConfig({
  e2e: {
    ...nxE2EPreset(__filename, {
      cypressDir: 'src',
      webServerCommands: {
        default: 'pnpm exec nx run teensyrom-ui:serve',
        production: 'pnpm exec nx run teensyrom-ui:serve-static',
      },
      ciWebServerCommand: 'pnpm exec nx run teensyrom-ui:serve-static',
      ciBaseUrl: 'http://localhost:4200',
    }),
    baseUrl: 'http://localhost:4200',

    // Screenshot configuration
    screenshotOnRunFailure: true,
    screenshotsFolder: 'dist/cypress/apps/teensyrom-ui-e2e/screenshots',

    // Video configuration
    video: true, // Always record for debugging
    videoCompression: 32, // Balance quality vs size (0-51, lower = better)
    videosFolder: 'dist/cypress/apps/teensyrom-ui-e2e/videos',
    videoUploadOnPasses: false, // Auto-delete videos for passing tests
  },
});
