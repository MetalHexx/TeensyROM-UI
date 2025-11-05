/// <reference types="cypress" />

/**
 * Device Refresh Error Handling E2E Tests
 *
 * Tests device refresh error scenarios with proper error message extraction from API
 * ProblemDetails responses and alert display.
 *
 * Key workflows tested:
 * - 404 errors when devices become unavailable
 * - ProblemDetails.title extraction for user-friendly error messages
 * - UI state transitions during error conditions
 * - Bootstrap error handling and recovery scenarios
 *
 * Fixtures and interceptors:
 * - singleDevice fixture for initial connected state
 * - Custom error interceptors for various HTTP status codes
 * - ProblemDetails response mocking
 */

import {
  navigateToDeviceView,
  clickRefreshDevices,
  verifyConnected as expectDeviceConnected,
  verifyDeviceCount,
  getDeviceCard,
  DEVICE_VIEW_SELECTORS,
  DEVICE_CARD_SELECTORS,
  expectDeviceCardVisible,
} from './test-helpers';
import {
  ALERT_SEVERITY,
  clearAllAlerts,
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
  waitForFindDevices,
  interceptFindDevicesNotFound,
  interceptFindDevicesInternalServerError,
  interceptFindDevicesWithNetworkError,
  interceptFindDevicesWithError,
} from '../../support/interceptors/findDevices.interceptors';
import { singleDevice } from '../../support/test-data/fixtures/devices.fixture';

describe('Device Refresh - Error Handling with ProblemDetails', () => {
  describe('404 Error: Devices Become Unavailable', () => {
    const errorMessage = 'No TeensyRom devices found.';

    beforeEach(() => {
      clearAllAlerts();
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForFindDevices();

      expectDeviceConnected(0);
      verifyDeviceCount(1);
    });

    it('should display ProblemDetails error message in alert when refresh returns 404', () => {
      expectDeviceCardVisible(0);
      expectDeviceConnected(0);

      interceptFindDevicesNotFound(errorMessage);
      clickRefreshDevices();
      waitForFindDevices();

      verifyAlertVisible();
      verifyAlertMessage(errorMessage);
      verifyAlertSeverity(ALERT_SEVERITY.ERROR);
    });

    it('should clear device list when refresh returns 404', () => {
      cy.visit(APP_ROUTES.devices);

      cy.get(DEVICE_CARD_SELECTORS.card, { timeout: 10000 }).should('have.length', 1);
      getDeviceCard(0).should('be.visible');

      interceptFindDevicesNotFound(errorMessage);
      clickRefreshDevices();
      waitForFindDevices();

      cy.get(DEVICE_CARD_SELECTORS.card).should('not.exist');

      cy.get(DEVICE_VIEW_SELECTORS.deviceList).within(() => {
        cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 0);
      });
    });

    it('should show empty state message when refresh returns 404', () => {
      getDeviceCard(0).should('be.visible');
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('not.exist');

      interceptFindDevicesNotFound(errorMessage);
      clickRefreshDevices();
      waitForFindDevices();

      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage)
        .should('be.visible')
        .and('contain.text', 'No devices found');
    });

    it('should hide busy dialog when refresh returns 404', () => {
      getDeviceCard(0).should('be.visible');

      interceptFindDevicesNotFound(errorMessage);
      clickRefreshDevices();
      waitForFindDevices();

      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
    });

    it('should extract error message from ProblemDetails.title over generic error', () => {
      interceptFindDevicesWithError(
        404,
        errorMessage,
        'Technical details: COM port scan completed with 0 devices detected'
      );

      clickRefreshDevices();
      waitForFindDevices();

      verifyAlertMessage(errorMessage);
      verifyAlertMessageDoesNotContain('Technical details');
    });

    it('should handle 404 with missing ProblemDetails.title by falling back to detail', () => {
      const detailMessage = 'No devices could be detected on the system';

      interceptFindDevicesWithError(404, detailMessage);

      clickRefreshDevices();
      waitForFindDevices();

      verifyAlertMessage(detailMessage);
    });

    it('should allow dismissing the error alert', () => {
      interceptFindDevicesNotFound(errorMessage);
      clickRefreshDevices();
      waitForFindDevices();

      verifyAlertVisible();
      dismissAlert();
      verifyAlertDismissed();

      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');
    });

    it('should auto-dismiss error alert after timeout', () => {
      interceptFindDevicesNotFound(errorMessage);
      clickRefreshDevices();
      waitForFindDevices();

      verifyAlertVisible();
      waitForAlertAutoDismiss();
      verifyAlertDismissed();
    });

    it('should allow recovery after 404 error when devices become available again', () => {
      interceptFindDevicesWithError(404, errorMessage);

      clickRefreshDevices();
      waitForFindDevices();

      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForFindDevices();

      expectDeviceConnected(0);
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('not.exist');
    });
  });

  describe('Other Error Scenarios', () => {
    beforeEach(() => {
      clearAllAlerts();
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForFindDevices();
    });

    it('should handle 500 Internal Server Error with ProblemDetails', () => {
      const errorTitle = 'Internal Server Error';

      interceptFindDevicesInternalServerError(errorTitle);
      clickRefreshDevices();
      waitForFindDevices();

      verifyAlertVisible();
      verifyAlertMessage(errorTitle);
      verifyAlertSeverity(ALERT_SEVERITY.ERROR);
    });

    it('should handle network errors gracefully', () => {
      interceptFindDevicesWithNetworkError();
      clickRefreshDevices();
      waitForFindDevices();

      verifyAlertVisible();
    });
  });

  describe('Bootstrap Error Handling', () => {
    const errorMessage = 'No TeensyRom devices found.';

    it('should hide busy dialog when bootstrap fails with 404', () => {
      interceptFindDevicesNotFound(errorMessage);
      navigateToDeviceView();
      waitForFindDevices();

      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');

      verifyAlertVisible();
      verifyAlertMessage(errorMessage);

      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage)
        .should('be.visible')
        .and('contain.text', 'No devices found');

      cy.get(DEVICE_VIEW_SELECTORS.deviceList).within(() => {
        cy.get(DEVICE_CARD_SELECTORS.card).should('not.exist');
      });
    });

    it('should hide busy dialog when bootstrap fails with 500', () => {
      const errorTitle = 'Internal Server Error';

      interceptFindDevicesInternalServerError(errorTitle);
      navigateToDeviceView();
      waitForFindDevices();

      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');

      verifyAlertVisible();
      verifyAlertMessage(errorTitle);
      verifyAlertSeverity(ALERT_SEVERITY.ERROR);
    });

    it('should hide busy dialog when bootstrap fails with network error', () => {
      interceptFindDevicesWithNetworkError();

      navigateToDeviceView();
      waitForFindDevices();

      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');
    });

    it('should complete bootstrap and show error state within reasonable time when API fails', () => {
      interceptFindDevicesWithError(404, errorMessage);

      const startTime = Date.now();
      navigateToDeviceView();
      waitForFindDevices();

      cy.then(() => {
        const elapsed = Date.now() - startTime;
        expect(elapsed).to.be.lessThan(2000);
      });

      verifyAlertVisible();
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
    });

    it('should allow recovery from bootstrap error', () => {
      interceptFindDevicesWithError(404, errorMessage);

      navigateToDeviceView();
      waitForFindDevices();

      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForFindDevices();

      expectDeviceConnected(0);
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('not.exist');
    });
  });
});
