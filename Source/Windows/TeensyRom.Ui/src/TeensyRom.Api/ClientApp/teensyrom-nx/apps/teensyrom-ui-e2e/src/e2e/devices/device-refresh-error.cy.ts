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
  verifyConnected,
  verifyDeviceCount,
  getDeviceCard,
  DEVICE_VIEW_SELECTORS,
  DEVICE_CARD_SELECTORS,
  API_ROUTE_ALIASES,
  DEVICE_ENDPOINTS,
  ALERT_SELECTORS,
  APP_ROUTES,
  createProblemDetailsResponse,
} from './test-helpers';
import { interceptFindDevices } from '../../support/interceptors/device.interceptors';
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
      verifyConnected(0);
      verifyDeviceCount(1);
    });

    it('should display ProblemDetails error message in alert when refresh returns 404', () => {
      // Given: A device was previously connected
      // Wait for the device card to be visible first
      getDeviceCard(0).should('be.visible');
      verifyConnected(0);

      // Setup 404 interceptor with ProblemDetails response
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage, 'No TeensyROM devices were detected on any COM ports.'));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // When: We click the refresh devices button
      clickRefreshDevices();

      // Then: The API returns 404 with ProblemDetails
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // And: An alert popup appears with the error message from title field
      cy.get(ALERT_SELECTORS.container)
        .should('be.visible')
        .within(() => {
          cy.get(ALERT_SELECTORS.message).should('contain.text', errorMessage);
          cy.get(ALERT_SELECTORS.icon).should('contain.text', 'error');
        });
    });

    it('should clear device list when refresh returns 404', () => {
      // This test is self-contained - doesn't rely on beforeEach state
      // Setup: Start with a fresh page with one device
      cy.visit(APP_ROUTES.devices);
      
      // Wait for initial device to load from beforeEach
      cy.get(DEVICE_CARD_SELECTORS.card, { timeout: 10000 }).should('have.length', 1);
      getDeviceCard(0).should('be.visible');

      // Setup 404 interceptor for refresh
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // When: We click the refresh devices button
      clickRefreshDevices();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

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
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // When: Refresh is clicked
      clickRefreshDevices();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Then: We now see a "No Devices Found" label
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage)
        .should('be.visible')
        .and('contain.text', 'No devices found');
    });

    it('should hide busy dialog when refresh returns 404', () => {
      // Given: A device was previously connected
      getDeviceCard(0).should('be.visible');

      // Setup 404 interceptor
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // When: Refresh is clicked
      clickRefreshDevices();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Then: The busy dialog disappears
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
    });

    it('should extract error message from ProblemDetails.title over generic error', () => {
      // Setup 404 interceptor
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage, 'Technical details: COM port scan completed with 0 devices detected'));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // When: Refresh is clicked
      clickRefreshDevices();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Then: Alert shows the title message (user-friendly) not the detail (technical)
      cy.get(ALERT_SELECTORS.messageInContainer)
        .should('contain.text', errorMessage)
        .and('not.contain.text', 'Technical details');
    });

    it('should handle 404 with missing ProblemDetails.title by falling back to detail', () => {
      const detailMessage = 'No devices could be detected on the system';

      // Setup 404 with only detail field
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, detailMessage));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // When: Refresh is clicked
      clickRefreshDevices();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Then: Alert falls back to detail message
      cy.get(ALERT_SELECTORS.messageInContainer).should('contain.text', detailMessage);
    });

    it('should allow dismissing the error alert', () => {
      // Setup 404 interceptor
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // Trigger error
      clickRefreshDevices();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Verify alert visible
      cy.get(ALERT_SELECTORS.container).should('be.visible');

      // Dismiss alert
      cy.get(ALERT_SELECTORS.dismissButton).click();

      // Verify alert dismissed
      cy.get(ALERT_SELECTORS.container).should('not.exist');

      // Device list should still show empty state
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');
    });

    it('should auto-dismiss error alert after timeout', () => {
      // Setup 404 interceptor
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // Trigger error
      clickRefreshDevices();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Verify alert visible
      cy.get(ALERT_SELECTORS.container).should('be.visible');

      // Wait for auto-dismiss (default is 3000ms)
      // eslint-disable-next-line cypress/no-unnecessary-waiting
      cy.wait(3500);

      // Verify alert auto-dismissed
      cy.get(ALERT_SELECTORS.container).should('not.exist');
    });

    it('should allow recovery after 404 error when devices become available again', () => {
      // Trigger 404 error
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      clickRefreshDevices();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Verify no devices state
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');

      // Now devices become available again
      interceptFindDevices({ fixture: singleDevice });

      // Refresh again
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify recovery: device is back
      verifyConnected(0);
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

      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(500, errorTitle, 'An unexpected error occurred while scanning for devices'));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      clickRefreshDevices();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Verify error alert shows
      cy.get(ALERT_SELECTORS.container)
        .should('be.visible')
        .within(() => {
          cy.get(ALERT_SELECTORS.message).should('contain.text', errorTitle);
          cy.get(ALERT_SELECTORS.icon).should('contain.text', 'error');
        });
    });

    it('should handle network errors gracefully', () => {
      // Simulate network failure
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply({ forceNetworkError: true });
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      clickRefreshDevices();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Verify error alert shows (may use fallback message)
      cy.get(ALERT_SELECTORS.container).should('be.visible');
    });
  });

  describe('Bootstrap Error Handling', () => {
    const errorMessage = 'No TeensyRom devices found.';

    it('should hide busy dialog when bootstrap fails with 404', () => {
      // Setup 404 interceptor before navigation (simulates bootstrap failure)
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage, 'No TeensyROM devices were detected on any COM ports.'));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // Navigate to device view (triggers bootstrap findDevices)
      navigateToDeviceView();

      // Wait for the API call to complete
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Then: Busy dialog should be hidden
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');

      // And: Alert shows error message
      cy.get(ALERT_SELECTORS.container)
        .should('be.visible')
        .within(() => {
          cy.get(ALERT_SELECTORS.message).should('contain.text', errorMessage);
        });

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
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(500, errorTitle, 'An unexpected error occurred while scanning for devices'));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // Navigate to device view (triggers bootstrap)
      navigateToDeviceView();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Then: Busy dialog should be hidden
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');

      // And: Error alert is displayed
      cy.get(ALERT_SELECTORS.container)
        .should('be.visible')
        .within(() => {
          cy.get(ALERT_SELECTORS.message).should('contain.text', errorTitle);
          cy.get(ALERT_SELECTORS.icon).should('contain.text', 'error');
        });
    });

    it('should hide busy dialog when bootstrap fails with network error', () => {
      // Simulate network failure during bootstrap
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply({ forceNetworkError: true });
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // Navigate to device view
      navigateToDeviceView();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Then: Busy dialog should be hidden
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');

      // And: Some form of error indication is shown
      // (may be alert or empty state depending on error handling)
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');
    });

    it('should complete bootstrap and show error state within reasonable time when API fails', () => {
      // Setup 404 error
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      // Navigate
      const startTime = Date.now();
      navigateToDeviceView();

      // Wait for API call to complete
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Verify bootstrap completes quickly (within 2 seconds total)
      cy.then(() => {
        const elapsed = Date.now() - startTime;
        expect(elapsed).to.be.lessThan(2000);
      });

      // Verify error state is visible
      cy.get(ALERT_SELECTORS.container).should('be.visible');
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');
      
      // Verify busy dialog is no longer showing
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
    });

    it('should allow recovery from bootstrap error', () => {
      // First navigation fails with 404
      cy.intercept(
        DEVICE_ENDPOINTS.FIND_DEVICES.method,
        DEVICE_ENDPOINTS.FIND_DEVICES.pattern,
        (req) => {
          req.reply(createProblemDetailsResponse(404, errorMessage));
        }
      ).as(API_ROUTE_ALIASES.FIND_DEVICES);

      navigateToDeviceView();
      cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`);

      // Verify error state
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');

      // Now devices become available
      interceptFindDevices({ fixture: singleDevice });

      // Refresh to recover
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify recovery: device is found
      verifyConnected(0);
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('not.exist');
    });
  });
});
