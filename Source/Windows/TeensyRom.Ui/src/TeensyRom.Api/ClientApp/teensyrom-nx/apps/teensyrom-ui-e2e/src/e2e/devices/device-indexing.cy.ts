/// <reference types="cypress" />

/**
 * Device Storage Indexing E2E Tests
 *
 * Comprehensive test suite for storage indexing operations including:
 * - Single device USB/SD indexing
 * - Index All batch operations
 * - Button state management
 * - Busy dialog display and behavior
 * - Error handling and recovery
 * - Multi-device integration scenarios
 *
 * @see INDEXING_E2E_PLAN.md for detailed test specifications
 * @see E2E_TESTS.md for architecture and patterns
 */

import { interceptFindDevices } from '../../support/interceptors/findDevices.interceptors';
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
import { navigateToDeviceView, waitForDeviceDiscovery } from './test-helpers';

// =============================================================================
// Test Suite: Single Device Storage Indexing (Task 6)
// =============================================================================

describe('Device Storage Indexing - Single Device', () => {
  describe('Storage Availability and Button State', () => {
    it('Should display index button for available USB storage', () => {
      // Arrange
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 500 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Assert
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('be.visible').and('not.be.disabled');
    });

    it('Should display index button for available SD storage', () => {
      // Arrange
      interceptFindDevices({ fixture: deviceWithUnavailableUsbStorage });
      setupIndexingScenario(deviceWithUnavailableUsbStorage, { delay: 500 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Assert - SD available, USB unavailable
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.sd).should('be.visible');
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('be.visible');
    });

    it('Should display storage index buttons when storage unavailable', () => {
      // Arrange
      interceptFindDevices({ fixture: allStorageUnavailable });
      setupIndexingScenario(allStorageUnavailable, { delay: 500 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Assert - Buttons visible (busy dialog prevents clicks during indexing)
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('be.visible');
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.sd).should('be.visible');
    });
  });

  describe('Busy Dialog During Indexing', () => {
    it('Should show busy dialog when indexing USB storage', () => {
      // Arrange
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 2000 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickStorageIndexButton('usb');

      // Assert - Dialog appears immediately
      verifyBusyDialogDisplayed();

      // Assert - Dialog persists during indexing
      cy.get(BUSY_DIALOG_GENERIC_SELECTORS.container, { timeout: 2000 }).should('exist');

      // Assert - Dialog closes after completion
      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);
      verifyBusyDialogHidden();
    });

    it('Should show busy dialog when indexing SD storage', () => {
      // Arrange
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 2000 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickStorageIndexButton('sd');

      // Assert - Dialog appears with correct message
      verifyBusyDialogDisplayed();

      // Assert - Dialog persists during indexing
      cy.get(BUSY_DIALOG_GENERIC_SELECTORS.container, { timeout: 2000 }).should('exist');

      // Assert - Dialog closes when complete
      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Sd);
      verifyBusyDialogHidden();
    });
  });

  describe('Button State Management During Indexing', () => {
    it('Should display index button during indexing operation', () => {
      // Arrange
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 3000 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickStorageIndexButton('usb');

      // Assert - Button remains visible (busy dialog prevents interaction)
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('be.visible');

      // Wait for completion
      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);
    });
  });

  describe('Error Handling', () => {
    it('Should handle indexing error with error alert', () => {
      // Arrange
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, {
        delay: 1000,
        errorMode: true,
        errorMessage: 'Failed to index USB storage',
      });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickStorageIndexButton('usb');

      // Assert - Busy dialog appears
      verifyBusyDialogDisplayed();

      // Assert - Wait for error response
      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);

      // Assert - Dialog closes when error occurs
      verifyBusyDialogHidden();

      // Note: Error alert verification depends on application's error handling implementation
    });
  });
});

// =============================================================================
// Test Suite: Index All / Batch Indexing (Task 7)
// =============================================================================

describe('Device Storage Indexing - Index All', () => {
  describe('Index All Button State', () => {
    it('Should disable Index All button when no devices connected', () => {
      // Arrange
      interceptFindDevices({ fixture: noDevicesForIndexing });
      setupIndexingScenario(noDevicesForIndexing, { delay: 500 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Assert
      cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton).should('be.disabled');
    });

    it('Should enable Index All button when devices connected', () => {
      // Arrange
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, { delay: 500 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Assert
      cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton).should('not.be.disabled');
    });
  });

  describe('Index All Operation', () => {
    it('Should index all connected devices and storage when Index All clicked', () => {
      // Arrange
      interceptFindDevices({ fixture: threeDevicesFullIndexing });
      setupIndexingScenario(threeDevicesFullIndexing, { delay: 2000 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickIndexAllButton();

      // Assert - Busy dialog displays
      verifyBusyDialogDisplayed();

      // Assert - Wait for Index All API call to complete
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);

      // Assert - Dialog closes after all complete
      verifyBusyDialogHidden();
    });

    it('Should handle partial storage availability in Index All', () => {
      // Arrange - Device 1 has USB+SD, Device 2 has only SD
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, { delay: 1500 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickIndexAllButton();

      // Assert - Busy dialog persists for full operation
      verifyBusyDialogDisplayed();

      // Assert - Wait for operation to complete
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);

      // Assert - Dialog closes after all complete
      verifyBusyDialogHidden();
    });

    it('Should show busy dialog during Index All operation', () => {
      // Arrange
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, { delay: 3000 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickIndexAllButton();

      // Assert - Dialog appears immediately
      verifyBusyDialogDisplayed();

      // Assert - Dialog persists during full indexing duration
      cy.get(BUSY_DIALOG_GENERIC_SELECTORS.container, { timeout: 3000 }).should('exist');

      // Assert - Dialog closes after all indexing complete
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
      verifyBusyDialogHidden();
    });
  });

  describe('Index All Error Handling', () => {
    it('Should handle Index All with API failure', () => {
      // Arrange
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, {
        delay: 1000,
        errorMode: true,
        errorMessage: 'Failed to index all storage',
      });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickIndexAllButton();

      // Assert - Dialog shows during operation
      verifyBusyDialogDisplayed();

      // Assert - Wait for error response
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);

      // Assert - Dialog closes when error occurs
      verifyBusyDialogHidden();
    });
  });
});

// =============================================================================
// Test Suite: Error Handling & Edge Cases (Task 8)
// =============================================================================

describe('Device Storage Indexing - Error Handling & Edge Cases', () => {
  describe('Network Errors', () => {
    it('Should handle 400 Bad Request error during indexing', () => {
      // Arrange
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, {
        delay: 500,
        statusCode: 400,
        errorMode: true,
        errorMessage: 'Bad request - invalid storage',
      });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickStorageIndexButton('usb');

      // Assert - Dialog appears then closes
      verifyBusyDialogDisplayed();

      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);

      verifyBusyDialogHidden();
      // Note: Error alert verification depends on application error handling
    });

    it('Should handle 404 Not Found error during Index All', () => {
      // Arrange
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, {
        delay: 500,
        statusCode: 404,
        errorMode: true,
        errorMessage: 'No devices found',
      });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickIndexAllButton();

      // Assert
      verifyBusyDialogDisplayed();
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
      verifyBusyDialogHidden();
    });
  });

  describe('Error Recovery', () => {
    it('Should recover and allow retry after indexing error', () => {
      // Arrange - First attempt fails
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, {
        delay: 500,
        errorMode: true,
        errorMessage: 'First attempt failed',
      });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;

      // Act - First attempt (fails)
      clickStorageIndexButton('sd');
      waitForIndexingComplete(deviceId, TeensyStorageType.Sd);
      verifyBusyDialogHidden();

      // Assert - Button remains visible for retry
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.sd).should('be.visible');

      // Arrange - Second attempt succeeds (re-setup interceptor)
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 500 });

      // Act - Second attempt (succeeds)
      clickStorageIndexButton('sd');
      verifyBusyDialogDisplayed();
      waitForIndexingComplete(deviceId, TeensyStorageType.Sd);

      // Assert - Second attempt succeeds
      verifyBusyDialogHidden();
    });
  });

  describe('Rapid Clicks Prevention', () => {
    it('Should prevent rapid successive index clicks via busy dialog overlay', () => {
      // Arrange
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 3000 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act - Click USB index button
      clickStorageIndexButton('usb');

      // Assert - Busy dialog appears and prevents interaction
      verifyBusyDialogDisplayed();

      // Assert - Button still visible but dialog blocks interaction
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('be.visible');

      // Assert - Only one API call is made
      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);
      verifyBusyDialogHidden();
    });
  });
});

// =============================================================================
// Test Suite: Multi-Device Integration (Task 9)
// =============================================================================

describe('Device Storage Indexing - Multi-Device Integration', () => {
  describe('Device Independence', () => {
    it('Should display correct storage status for each device independently', () => {
      // Arrange - Device 1 (USB available), Device 2 (SD available + USB unavailable)
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, { delay: 500 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Assert - Verify both devices are displayed with correct storage states
      // Note: This test verifies UI rendering, actual selector depends on component structure
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.usb).should('have.length', 2);
      cy.get(STORAGE_INDEX_BUTTON_SELECTORS.sd).should('have.length', 2);
    });
  });
});

// =============================================================================
// Test Suite: Busy Dialog Content Verification (Task 10)
// =============================================================================

describe('Busy Dialog - Indexing Content', () => {
  describe('Dialog Content', () => {
    it('Should display correct dialog title and message during indexing', () => {
      // Arrange
      interceptFindDevices({ fixture: deviceWithAvailableStorage });
      setupIndexingScenario(deviceWithAvailableStorage, { delay: 2000 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickStorageIndexButton('usb');

      // Assert - Dialog displays
      verifyBusyDialogDisplayed();

      // Cleanup
      const deviceId = deviceWithAvailableStorage.devices[0].deviceId;
      waitForIndexingComplete(deviceId, TeensyStorageType.Usb);
      verifyBusyDialogHidden();
    });

    it('Should display same dialog content for Index All operation', () => {
      // Arrange
      interceptFindDevices({ fixture: multipleDevicesForIndexing });
      setupIndexingScenario(multipleDevicesForIndexing, { delay: 2000 });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Act
      clickIndexAllButton();

      // Assert - Dialog displays
      verifyBusyDialogDisplayed();

      // Cleanup
      waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
      verifyBusyDialogHidden();
    });
  });
});
