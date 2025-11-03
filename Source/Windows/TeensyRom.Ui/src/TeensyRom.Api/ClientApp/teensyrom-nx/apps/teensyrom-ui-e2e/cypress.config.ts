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

    // Video configuration - disabled to avoid Windows file-locking issues
    video: false,

    // Reporter configuration - for JSON test reports (agent consumption)
    // Use require.resolve to handle pnpm workspace module resolution
    reporter: require.resolve('cypress-mochawesome-reporter'),
    reporterOptions: {
      reportDir: 'dist/cypress/apps/teensyrom-ui-e2e/reports',
      charts: false,
      reportPageTitle: 'TeensyROM UI E2E Test Report',
      embeddedScreenshots: false,
      inlineAssets: false,
      saveJson: true,
      saveHtml: false,
      autoOpen: false,
      // Prevent copying screenshots to report folder (JSON-only, no HTML needed)
      screenshotsFolder: null,
    },

    setupNodeEvents(on, config) {
      // Register mochawesome reporter plugin with resolved path
      // eslint-disable-next-line @typescript-eslint/no-require-imports
      const mochawesomeReporter = require('cypress-mochawesome-reporter/plugin');
      mochawesomeReporter(on);
      return config;
    },
  },
});
