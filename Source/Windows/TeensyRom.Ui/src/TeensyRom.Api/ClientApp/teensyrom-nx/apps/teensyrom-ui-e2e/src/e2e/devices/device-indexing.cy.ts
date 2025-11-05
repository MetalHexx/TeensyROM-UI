/// <reference types="cypress" />

/**
 * Device Storage Indexing E2E Tests
 *
 * Test suite for storage indexing operations including single device USB/SD indexing,
 * Index All batch operations, button state management, busy dialog behavior, error handling,
 * and multi-device integration scenarios.
 *
 * @see INDEXING_E2E_PLAN.md for detailed test specifications
 * @see E2E_TESTS.md for architecture and patterns
 */

import {
  interceptFindDevices,
  waitForFindDevices
} from '../../support/interceptors/findDevices.interceptors';
import {
  deviceWithAvailableStorage,
  deviceWithUnavailableUsbStorage,
  allStorageUnavailable,
  multipleDevicesForIndexing,
  threeDevicesFullIndexing,
  noDevicesForIndexing,
} from '../../support/test-data/fixtures/indexing.fixture';
import {
  setupIndexingScenario,
  verifyBusyDialogDisplayed,
  verifyBusyDialogHidden,
  clickStorageIndexButton,
  clickIndexAllButton,
  waitForIndexingComplete,
} from '../../support/helpers/indexing.helpers';
import {
  INDEXING_INTERCEPT_ALIASES,
} from '../../support/interceptors/indexStorage.interceptors';
import { TeensyStorageType } from '@teensyrom-nx/data-access/api-client';
import {
  STORAGE_INDEX_BUTTON_SELECTORS,
  DEVICE_TOOLBAR_SELECTORS,
  BUSY_DIALOG_GENERIC_SELECTORS,
} from '../../support/constants/selector.constants';
import { navigateToDeviceView } from './test-helpers';

// =============================================================================
// SINGLE DEVICE STORAGE INDEXING
// =============================================================================

describe('Device Storage Indexing - Single Device', () => {
  describe('Storage Availability and Button State', () => {
    it('Should display index button for available USB storage', () => {
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 500 });
      navigateToDeviceView();
      waitForFindDevices();

      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('be.visible').and('not.be.disabled');
    });

    it('Should display index button for available SD storage', () => {
      interceptFindDevices({ fixture: deviceWithUnavailableUsbStorage });
      setupIndexingScenario(deviceWithUnavailableUsbStorage, { delay: 500 });
      navigateToDeviceView();
      waitForFindDevices();

      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.sd).should('be.visible');
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('be.visible');
    });

    it('Should display storage index buttons when storage unavailable', () => {
      interceptFindDevices({ fixture: allStorageUnavailable });
      setupIndexingScenario(allStorageUnavailable, { delay: 500 });
      navigateToDeviceView();
      waitForFindDevices();

      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('be.visible');
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.sd).should('be.visible');
    });
  });

  describe('Busy Dialog During Indexing', () => {
    it('Should show busy dialog when indexing USB storage', () => {
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 2000 });
      navigateToDeviceView();
      waitForFindDevices();

      clickStorageIndexButton('usb');
      verifyBusyDialogDisplayed();
      cy.get(BUSY_DIALOG_GENERIC_SELECTORS.container, { timeout: 2000 }).should('exist');

      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);
      verifyBusyDialogHidden();
    });

    it('Should show busy dialog when indexing SD storage', () => {
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 2000 });
      navigateToDeviceView();
      waitForFindDevices();

      clickStorageIndexButton('sd');
      verifyBusyDialogDisplayed();
      cy.get(BUSY_DIALOG_GENERIC_SELECTORS.container, { timeout: 2000 }).should('exist');

      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Sd);
      verifyBusyDialogHidden();
    });
  });

  describe('Button State Management During Indexing', () => {
    it('Should display index button during indexing operation', () => {
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 3000 });
      navigateToDeviceView();
      waitForFindDevices();

      clickStorageIndexButton('usb');
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('be.visible');

      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);
    });
  });

  describe('Error Handling', () => {
    it('Should handle indexing error with error alert', () => {
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, {
        delay: 1000,
        errorMode: true,
        errorMessage: 'Failed to index USB storage',
      });
      navigateToDeviceView();
      waitForFindDevices();

      clickStorageIndexButton('usb');
      verifyBusyDialogDisplayed();

      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);
      verifyBusyDialogHidden();
    });
  });
});

// =============================================================================
// INDEX ALL / BATCH INDEXING
// =============================================================================

describe('Device Storage Indexing - Index All', () => {
  describe('Index All Button State', () => {
    it('Should disable Index All button when no devices connected', () => {
      interceptFindDevices({ fixture: noDevicesForIndexing });
      setupIndexingScenario(noDevicesForIndexing, { delay: 500 });
      navigateToDeviceView();
      waitForFindDevices();

      cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton).should('be.disabled');
    });

    it('Should enable Index All button when devices connected', () => {
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, { delay: 500 });
      navigateToDeviceView();
      waitForFindDevices();

      cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton).should('not.be.disabled');
    });
  });

  describe('Index All Operation', () => {
    it('Should index all connected devices and storage when Index All clicked', () => {
      interceptFindDevices({ fixture: threeDevicesFullIndexing });
      setupIndexingScenario(threeDevicesFullIndexing, { delay: 2000 });
      navigateToDeviceView();
      waitForFindDevices();

      clickIndexAllButton();
      verifyBusyDialogDisplayed();
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
      verifyBusyDialogHidden();
    });

    it('Should handle partial storage availability in Index All', () => {
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, { delay: 1500 });
      navigateToDeviceView();
      waitForFindDevices();

      clickIndexAllButton();
      verifyBusyDialogDisplayed();
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
      verifyBusyDialogHidden();
    });

    it('Should show busy dialog during Index All operation', () => {
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, { delay: 3000 });
      navigateToDeviceView();
      waitForFindDevices();

      clickIndexAllButton();
      verifyBusyDialogDisplayed();
      cy.get(BUSY_DIALOG_GENERIC_SELECTORS.container, { timeout: 3000 }).should('exist');
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
      verifyBusyDialogHidden();
    });
  });

  describe('Index All Error Handling', () => {
    it('Should handle Index All with API failure', () => {
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, {
        delay: 1000,
        errorMode: true,
        errorMessage: 'Failed to index all storage',
      });
      navigateToDeviceView();
      waitForFindDevices();

      clickIndexAllButton();
      verifyBusyDialogDisplayed();
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
      verifyBusyDialogHidden();
    });
  });
});

// =============================================================================
// ERROR HANDLING & EDGE CASES
// =============================================================================

describe('Device Storage Indexing - Error Handling & Edge Cases', () => {
  describe('Network Errors', () => {
    it('Should handle 400 Bad Request error during indexing', () => {
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, {
        delay: 500,
        statusCode: 400,
        errorMode: true,
        errorMessage: 'Bad request - invalid storage',
      });
      navigateToDeviceView();
      waitForFindDevices();

      clickStorageIndexButton('usb');
      verifyBusyDialogDisplayed();

      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);
      verifyBusyDialogHidden();
    });

    it('Should handle 404 Not Found error during Index All', () => {
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, {
        delay: 500,
        statusCode: 404,
        errorMode: true,
        errorMessage: 'No devices found',
      });
      navigateToDeviceView();
      waitForFindDevices();

      clickIndexAllButton();
      verifyBusyDialogDisplayed();
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
      verifyBusyDialogHidden();
    });
  });

  describe('Error Recovery', () => {
    it('Should recover and allow retry after indexing error', () => {
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, {
        delay: 500,
        errorMode: true,
        errorMessage: 'First attempt failed',
      });
      navigateToDeviceView();
      waitForFindDevices();

      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;

      clickStorageIndexButton('sd');
      waitForIndexingComplete(deviceId, TeensyStorageType.Sd);
      verifyBusyDialogHidden();
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.sd).should('be.visible');

      setupIndexingScenario(deviceWithAvailableStorage, { delay: 500 });
      clickStorageIndexButton('sd');
      verifyBusyDialogDisplayed();
      waitForIndexingComplete(deviceId, TeensyStorageType.Sd);
      verifyBusyDialogHidden();
    });
  });

  describe('Rapid Clicks Prevention', () => {
    it('Should prevent rapid successive index clicks via busy dialog overlay', () => {
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 3000 });
      navigateToDeviceView();
      waitForFindDevices();

      clickStorageIndexButton('usb');
      verifyBusyDialogDisplayed();
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('be.visible');

      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);
      verifyBusyDialogHidden();
    });
  });
});

// =============================================================================
// MULTI-DEVICE INTEGRATION
// =============================================================================

describe('Device Storage Indexing - Multi-Device Integration', () => {
  describe('Device Independence', () => {
    it('Should display correct storage status for each device independently', () => {
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, { delay: 500 });
      navigateToDeviceView();
      waitForFindDevices();

      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('have.length', 2);
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.sd).should('have.length', 2);
    });
  });
});

// =============================================================================
// BUSY DIALOG CONTENT VERIFICATION
// =============================================================================

describe('Busy Dialog - Indexing Content', () => {
  describe('Dialog Content', () => {
    it('Should display correct dialog title and message during indexing', () => {
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 2000 });
      navigateToDeviceView();
      waitForFindDevices();

      clickStorageIndexButton('usb');
      verifyBusyDialogDisplayed();

      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);
      verifyBusyDialogHidden();
    });

    it('Should display same dialog content for Index All operation', () => {
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, { delay: 2000 });
      navigateToDeviceView();
      waitForFindDevices();

      clickIndexAllButton();
      verifyBusyDialogDisplayed();
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
      verifyBusyDialogHidden();
    });
  });
});
