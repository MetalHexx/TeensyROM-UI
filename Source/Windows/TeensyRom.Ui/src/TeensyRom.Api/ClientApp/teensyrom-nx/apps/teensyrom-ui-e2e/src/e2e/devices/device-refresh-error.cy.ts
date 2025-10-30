/// <reference types="cypress" />

/**
 * Device Refresh Error Handling E2E Tests
 *
 * Tests device refresh error scenarios with proper error message extraction
 * from API ProblemDetails responses and alert display.
 *
 * **Key Workflows**:
 * - Refresh fails with 404 error when devices become unavailable
 * - Error message from ProblemDetails.title is displayed in alert
 * - Device cards disappear when no devices found
 * - "No Devices Found" message is shown
 * - Busy dialog is dismissed after error
 *
 * **Fixtures Used**:
 * - singleDevice: Single connected device for initial state
 * - Custom 404 ProblemDetails response for error scenario
 *
 * **Interceptors Used**:
 * - interceptFindDevices(): Mock device discovery
 * - Custom 404 interceptor with ProblemDetails
 */

import {
  navigateToDeviceView,
  waitForDeviceDiscovery,
  clickRefreshDevices,
  verifyConnected as expectDeviceConnected,
  verifyDeviceCount,
  getDeviceCard,
  DEVICE_VIEW_SELECTORS,
  DEVICE_CARD_SELECTORS,
  createProblemDetailsResponse,
  expectDeviceCardVisible,
} from './test-helpers';
import {
  ALERT_SEVERITY,
  dismissAlert,
  verifyAlertDismissed,
  verifyAlertMessage,
  verifyAlertMessageDoesNotContain,
  verifyAlertSeverity,
  verifyAlertVisible,
  waitForAlertAutoDismiss,
} from '../../support/helpers/alert.helpers';
import { APP_ROUTES } from '../../support/constants/app-routes.constants';
import {
  interceptFindDevices,
  FIND_DEVICES_ENDPOINT,
  FIND_DEVICES_ALIAS,
  interceptFindDevicesNotFound,
  interceptFindDevicesInternalServerError,
  interceptFindDevicesWithNetworkError
} from '../../support/interceptors/findDevices.interceptors';
import { singleDevice } from '../../support/test-data/fixtures/devices.fixture';

describe('Device Refresh - Error Handling with ProblemDetails', () => {
  describe('404 Error: Devices Become Unavailable', () => {
    const errorMessage = 'No TeensyRom devices found.';

    beforeEach(() => {
      // Initial state: One connected device is available
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify initial connected state
      expectDeviceConnected(0);
      verifyDeviceCount(1);
    });

    it('should display ProblemDetails error message in alert when refresh returns 404', () => {
      // Given: A device was previously connected
      expectDeviceCardVisible(0);
      expectDeviceConnected(0);

      // Setup 404 interceptor with ProblemDetails response
      interceptFindDevicesNotFound(errorMessage);

      // When: We click the refresh devices button
      clickRefreshDevices();

      // Then: The API returns 404 with ProblemDetails
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // And: An alert popup appears with the error message from title field
      verifyAlertVisible();
      verifyAlertMessage(errorMessage);
      verifyAlertSeverity(ALERT_SEVERITY.ERROR);
    });

    it('should clear device list when refresh returns 404', () => {
      // This test is self-contained - doesn't rely on beforeEach state
      // Setup: Start with a fresh page with one device
      cy.visit(APP_ROUTES.devices);
      
      // Wait for initial device to load from beforeEach
      cy.get(DEVICE_CARD_SELECTORS.card, { timeout: 10000 }).should('have.length', 1);
      getDeviceCard(0).should('be.visible');

      // Setup 404 interceptor for refresh
      interceptFindDevicesNotFound(errorMessage);

      // When: We click the refresh devices button
      clickRefreshDevices();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Then: The device cards should be gone
      cy.get(DEVICE_CARD_SELECTORS.card).should('not.exist');

      // And: Device count should be 0
      cy.get(DEVICE_VIEW_SELECTORS.deviceList).within(() => {
        cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 0);
      });
    });

    it('should show empty state message when refresh returns 404', () => {
      // Given: A device was previously connected
      getDeviceCard(0).should('be.visible');
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('not.exist');

      // Setup 404 interceptor
      interceptFindDevicesNotFound(errorMessage);

      // When: Refresh is clicked
      clickRefreshDevices();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Then: We now see a "No Devices Found" label
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage)
        .should('be.visible')
        .and('contain.text', 'No devices found');
    });

    it('should hide busy dialog when refresh returns 404', () => {
      // Given: A device was previously connected
      getDeviceCard(0).should('be.visible');

      // Setup 404 interceptor
      interceptFindDevicesNotFound(errorMessage);

      // When: Refresh is clicked
      clickRefreshDevices();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Then: The busy dialog disappears
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
    });

    it('should extract error message from ProblemDetails.title over generic error', () => {
      // Setup 404 interceptor
      cy.intercept(
        FIND_DEVICES_ENDPOINT.method,
        FIND_DEVICES_ENDPOINT.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage, 'Technical details: COM port scan completed with 0 devices detected'));
        }
      ).as(FIND_DEVICES_ALIAS);

      // When: Refresh is clicked
      clickRefreshDevices();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Then: Alert shows the title message (user-friendly) not the detail (technical)
      verifyAlertMessage(errorMessage);
      verifyAlertMessageDoesNotContain('Technical details');
    });

    it('should handle 404 with missing ProblemDetails.title by falling back to detail', () => {
      const detailMessage = 'No devices could be detected on the system';

      // Setup 404 with only detail field
      cy.intercept(
        FIND_DEVICES_ENDPOINT.method,
        FIND_DEVICES_ENDPOINT.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, detailMessage));
        }
      ).as(FIND_DEVICES_ALIAS);

      // When: Refresh is clicked
      clickRefreshDevices();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Then: Alert falls back to detail message
      verifyAlertMessage(detailMessage);
    });

    it('should allow dismissing the error alert', () => {
      // Setup 404 interceptor
      interceptFindDevicesNotFound(errorMessage);

      // Trigger error
      clickRefreshDevices();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Verify alert visible
      verifyAlertVisible();

      // Dismiss alert
      dismissAlert();

      // Verify alert dismissed
      verifyAlertDismissed();

      // Device list should still show empty state
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');
    });

    it('should auto-dismiss error alert after timeout', () => {
      // Setup 404 interceptor
      interceptFindDevicesNotFound(errorMessage);

      // Trigger error
      clickRefreshDevices();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Verify alert visible
      verifyAlertVisible();

      // Wait for auto-dismiss (default is 3000ms)
      waitForAlertAutoDismiss();
      verifyAlertDismissed();
    });

    it('should allow recovery after 404 error when devices become available again', () => {
      // Trigger 404 error
      cy.intercept(
        FIND_DEVICES_ENDPOINT.method,
        FIND_DEVICES_ENDPOINT.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage));
        }
      ).as(FIND_DEVICES_ALIAS);

      clickRefreshDevices();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Verify no devices state
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');

      // Now devices become available again
      interceptFindDevices({ fixture: singleDevice });

      // Refresh again
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify recovery: device is back
      expectDeviceConnected(0);
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('not.exist');
    });
  });

  describe('Other Error Scenarios', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should handle 500 Internal Server Error with ProblemDetails', () => {
      const errorTitle = 'Internal Server Error';

      interceptFindDevicesInternalServerError(errorTitle);

      clickRefreshDevices();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Verify error alert shows
      verifyAlertVisible();
      verifyAlertMessage(errorTitle);
      verifyAlertSeverity(ALERT_SEVERITY.ERROR);
    });

    it('should handle network errors gracefully', () => {
      // Simulate network failure
      interceptFindDevicesWithNetworkError();

      clickRefreshDevices();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Verify error alert shows (may use fallback message)
      verifyAlertVisible();
    });
  });

  describe('Bootstrap Error Handling', () => {
    const errorMessage = 'No TeensyRom devices found.';

    it('should hide busy dialog when bootstrap fails with 404', () => {
      // Setup 404 interceptor before navigation (simulates bootstrap failure)
      interceptFindDevicesNotFound(errorMessage);

      // Navigate to device view (triggers bootstrap findDevices)
      navigateToDeviceView();

      // Wait for the API call to complete
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Then: Busy dialog should be hidden
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');

      // And: Alert shows error message
      verifyAlertVisible();
      verifyAlertMessage(errorMessage);

      // And: Empty state message is shown
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage)
        .should('be.visible')
        .and('contain.text', 'No devices found');

      // And: No device cards are present
      cy.get(DEVICE_VIEW_SELECTORS.deviceList).within(() => {
        cy.get(DEVICE_CARD_SELECTORS.card).should('not.exist');
      });
    });

    it('should hide busy dialog when bootstrap fails with 500', () => {
      const errorTitle = 'Internal Server Error';

      // Setup 500 error interceptor before navigation
      interceptFindDevicesInternalServerError(errorTitle);

      // Navigate to device view (triggers bootstrap)
      navigateToDeviceView();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Then: Busy dialog should be hidden
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');

      // And: Error alert is displayed
      verifyAlertVisible();
      verifyAlertMessage(errorTitle);
      verifyAlertSeverity(ALERT_SEVERITY.ERROR);
    });

    it('should hide busy dialog when bootstrap fails with network error', () => {
      // Simulate network failure during bootstrap
      cy.intercept(
        FIND_DEVICES_ENDPOINT.method,
        FIND_DEVICES_ENDPOINT.pattern,
        (req) => {
          req.reply({ forceNetworkError: true });
        }
      ).as(FIND_DEVICES_ALIAS);

      // Navigate to device view
      navigateToDeviceView();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Then: Busy dialog should be hidden
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');

      // And: Some form of error indication is shown
      // (may be alert or empty state depending on error handling)
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');
    });

    it('should complete bootstrap and show error state within reasonable time when API fails', () => {
      // Setup 404 error
      cy.intercept(
        FIND_DEVICES_ENDPOINT.method,
        FIND_DEVICES_ENDPOINT.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage));
        }
      ).as(FIND_DEVICES_ALIAS);

      // Navigate
      const startTime = Date.now();
      navigateToDeviceView();

      // Wait for API call to complete
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Verify bootstrap completes quickly (within 2 seconds total)
      cy.then(() => {
        const elapsed = Date.now() - startTime;
        expect(elapsed).to.be.lessThan(2000);
      });

      // Verify error state is visible
      verifyAlertVisible();
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');
      
      // Verify busy dialog is no longer showing
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
    });

    it('should allow recovery from bootstrap error', () => {
      // First navigation fails with 404
      cy.intercept(
        FIND_DEVICES_ENDPOINT.method,
        FIND_DEVICES_ENDPOINT.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage));
        }
      ).as(FIND_DEVICES_ALIAS);

      navigateToDeviceView();
      cy.wait(`@${FIND_DEVICES_ALIAS}`);

      // Verify error state
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');

      // Now devices become available
      interceptFindDevices({ fixture: singleDevice });

      // Refresh to recover
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify recovery: device is found
      expectDeviceConnected(0);
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('not.exist');
    });
  });
});
