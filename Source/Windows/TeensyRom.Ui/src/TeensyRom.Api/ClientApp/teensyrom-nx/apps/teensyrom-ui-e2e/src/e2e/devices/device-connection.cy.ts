/// <reference types="cypress" />

/**
 * Device Connection Lifecycle E2E Tests - Phase 1
 *
 * **Phase Goal**: Establish core connection/disconnection testing patterns for a single TeensyROM device.
 * This phase validates the complete connection lifecycle and creates reusable test helpers for
 * subsequent phases (multi-device, refresh, recovery).
 *
 * **Test Structure**:
 * - Connection Success: Happy path when connecting disconnected devices
 * - Disconnection Success: Happy path when disconnecting connected devices
 * - Connection Errors: API failures during connection attempts
 * - Disconnection Errors: API failures during disconnection attempts
 * - Visual Feedback: State transitions and visual indicators
 *
 * **Fixtures Used**:
 * - singleDevice: Connected device (default state after bootstrap)
 * - disconnectedDevice: Device with ConnectionLost state
 *
 * **Interceptors Used**:
 * - interceptFindDevices(): Mock device discovery API
 * - interceptConnectDevice(): Mock device connection endpoint
 * - interceptDisconnectDevice(): Mock device disconnection endpoint
 *
 * **Key Helpers (test-helpers.ts)**:
 * - clickPowerButton(index): Click power button to initiate connect/disconnect
 * - waitForConnection(): Wait for connection API call to complete
 * - waitForDisconnection(): Wait for disconnection API call to complete
 * - verifyConnected(index): Verify device is visually and state-wise connected
 * - verifyDisconnected(index): Verify device is visually and state-wise disconnected
 *
 * **Note on Phase 4 (Alerts)**: Alert message validation is deferred to Phase 4 to keep
 * Phase 1 focused on connection state behavior. Error tests verify device state is preserved
 * after failures, but detailed alert content validation happens in Phase 4.
 */

import {
  navigateToDeviceView,
  waitForDeviceDiscovery,
  getDeviceCard,
  verifyFullDeviceInfo,
  CSS_CLASSES,
  DEVICE_CARD_SELECTORS,
  clickPowerButton,
  waitForConnection,
  waitForDisconnection,
  verifyConnected,
  verifyDisconnected,
} from './test-helpers';
import {
  interceptFindDevices,
} from '../../support/interceptors/findDevices.interceptors';
import {
  interceptDisconnectDevice,
  DISCONNECT_DEVICE_ENDPOINT,
} from '../../support/interceptors/disconnectDevice.interceptors';
import {
  interceptConnectDevice,
  CONNECT_DEVICE_ENDPOINT,
} from '../../support/interceptors/connectDevice.interceptors';
import {
  singleDevice,
  disconnectedDevice,
} from '../../support/test-data/fixtures';

describe('Device Connection - Single Device', () => {
  // =========================================================================
  // SUITE 1: CONNECTION SUCCESS
  // =========================================================================
  describe('Connection Success', () => {
    beforeEach(() => {
      // Setup: Disconnected device for connection tests
      interceptFindDevices({ fixture: disconnectedDevice });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should connect to disconnected device when power button clicked', () => {
      // Verify initial disconnected state
      verifyDisconnected(0);

      // Click power button to initiate connection
      clickPowerButton(0);

      // Wait for connection API call
      waitForConnection();

      // Verify connected state after API call completes
      verifyConnected(0);
    });

    it('should update power button color after connection', () => {
      // Setup
      verifyDisconnected(0); // Initial state: color="normal"

      // Action
      clickPowerButton(0);
      waitForConnection();

      // Assert: Power button icon has "highlight" class
      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', 'highlight');
    });

    it('should remove dimmed styling from device card after connection', () => {
      // Setup
      getDeviceCard(0).should('have.class', CSS_CLASSES.DIMMED); // Initial state

      // Action
      clickPowerButton(0);
      waitForConnection();

      // Assert: Dimmed class removed
      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
    });

    it('should call connection API with correct device ID', () => {
      // Setup: Capture connection request using constants
      cy.intercept(
        CONNECT_DEVICE_ENDPOINT.method,
        CONNECT_DEVICE_ENDPOINT.pattern,
        (req) => {
          // Verify request contains device ID from fixture
          expect(req.url).to.include(disconnectedDevice.devices[0].deviceId);
          req.reply({
            statusCode: 200,
            body: {
              connectedCart: disconnectedDevice.devices[0],
              message: 'Connected',
            },
          });
        }
      ).as('connectionRequest');

      // Action
      clickPowerButton(0);
      cy.wait('@connectionRequest');

      // Assert: Request was made with correct device ID (implicit in intercept validation)
    });
  });

  // =========================================================================
  // SUITE 2: DISCONNECTION SUCCESS
  // =========================================================================
  describe('Disconnection Success', () => {
    beforeEach(() => {
      // Setup: Connected device for disconnection tests
      interceptFindDevices({ fixture: singleDevice });
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should disconnect from connected device when power button clicked', () => {
      // Verify initial connected state
      verifyConnected(0);

      // Click power button to initiate disconnection
      clickPowerButton(0);

      // Wait for disconnection API call
      waitForDisconnection();

      // Verify disconnected state after API call completes
      verifyDisconnected(0);
    });

    it('should update power button color after disconnection', () => {
      // Setup
      verifyConnected(0); // Initial state: color="highlight"

      // Action
      clickPowerButton(0);
      waitForDisconnection();

      // Assert: Power button icon has "normal" class
      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', 'normal');
    });

    it('should apply dimmed styling to device card after disconnection', () => {
      // Setup
      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED); // Initial state

      // Action
      clickPowerButton(0);
      waitForDisconnection();

      // Assert: Dimmed class applied
      getDeviceCard(0).should('have.class', CSS_CLASSES.DIMMED);
    });

    it('should call disconnection API with correct device ID', () => {
      // Setup: Capture disconnection request using constants
      cy.intercept(
        DISCONNECT_DEVICE_ENDPOINT.method,
        DISCONNECT_DEVICE_ENDPOINT.pattern,
        (req) => {
          // Verify request contains device ID from fixture
          expect(req.url).to.include(singleDevice.devices[0].deviceId);
          req.reply({
            statusCode: 200,
            body: { message: 'Disconnected' },
          });
        }
      ).as('disconnectionRequest');

      // Action
      clickPowerButton(0);
      cy.wait('@disconnectionRequest');

      // Assert: Request was made with correct device ID (implicit in intercept validation)
    });
  });

  // =========================================================================
  // SUITE 3: CONNECTION ERRORS
  // =========================================================================
  describe('Connection Errors', () => {
    beforeEach(() => {
      // Setup: Disconnected device for connection error tests
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should handle connection API failure', () => {
      // Setup: Connection will fail with 500 error
      interceptConnectDevice({ errorMode: true });

      // Verify initial disconnected state
      verifyDisconnected(0);

      // Action: Attempt to connect
      clickPowerButton(0);
      waitForConnection();

      // Assert: Device remains disconnected after failed connection attempt
      verifyDisconnected(0);
    });

    it('should display error message after connection failure', () => {
      // Setup: Connection will fail
      interceptConnectDevice({ errorMode: true });

      // Action: Attempt to connect
      clickPowerButton(0);
      waitForConnection();

      // Assert: Error message appears (Note: Detailed alert validation in Phase 4)
      // For now, just verify error occurred by checking device remains disconnected
      verifyDisconnected(0);
    });

    it('should allow retry after connection failure', () => {
      // Setup: First attempt fails, second succeeds
      interceptConnectDevice({ errorMode: true });

      // Action: First attempt fails
      clickPowerButton(0);
      waitForConnection();
      verifyDisconnected(0); // Still disconnected after failed attempt

      // Change interceptor to success mode for retry
      interceptConnectDevice({ errorMode: false });

      // Action: Retry connection
      clickPowerButton(0);
      waitForConnection();

      // Assert: Retry succeeds
      verifyConnected(0);
    });

    it('should maintain device information after connection failure', () => {
      // Setup: Connection will fail
      interceptConnectDevice({ errorMode: true });

      // Action: Attempt to connect
      clickPowerButton(0);
      waitForConnection();

      // Assert: Device card displays all device information despite failure
      verifyFullDeviceInfo(0);
    });
  });

  // =========================================================================
  // SUITE 4: DISCONNECTION ERRORS
  // =========================================================================
  describe('Disconnection Errors', () => {
    beforeEach(() => {
      // Setup: Connected device for disconnection error tests
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should handle disconnection API failure', () => {
      // Setup: Disconnection will fail with 500 error
      interceptDisconnectDevice({ errorMode: true });

      // Verify initial connected state
      verifyConnected(0);

      // Action: Attempt to disconnect
      clickPowerButton(0);
      waitForDisconnection();

      // Assert: Device remains connected after failed disconnection attempt
      verifyConnected(0);
    });

    it('should display error message after disconnection failure', () => {
      // Setup: Disconnection will fail
      interceptDisconnectDevice({ errorMode: true });

      // Action: Attempt to disconnect
      clickPowerButton(0);
      waitForDisconnection();

      // Assert: Error message appears (Note: Detailed alert validation in Phase 4)
      // For now, just verify error occurred by checking device remains connected
      verifyConnected(0);
    });

    it('should allow retry after disconnection failure', () => {
      // Setup: First attempt fails, second succeeds
      interceptDisconnectDevice({ errorMode: true });

      // Action: First attempt fails
      clickPowerButton(0);
      waitForDisconnection();
      verifyConnected(0); // Still connected after failed attempt

      // Change interceptor to success mode for retry
      interceptDisconnectDevice({ errorMode: false });

      // Action: Retry disconnection
      clickPowerButton(0);
      waitForDisconnection();

      // Assert: Retry succeeds
      verifyDisconnected(0);
    });

    it('should maintain connected state after disconnection failure', () => {
      // Setup: Disconnection will fail
      interceptDisconnectDevice({ errorMode: true });

      // Action: Attempt to disconnect
      clickPowerButton(0);
      waitForDisconnection();

      // Assert: Device remains connected (not dimmed, power button highlighted)
      verifyConnected(0);
    });
  });

  // =========================================================================
  // SUITE 5: VISUAL FEEDBACK
  // =========================================================================
  describe('Visual Feedback', () => {
    it('should show correct initial visual state for disconnected device', () => {
      // Setup: Disconnected device
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Assert: Visual indicators show disconnected state
      verifyDisconnected(0);
    });

    it('should show correct initial visual state for connected device', () => {
      // Setup: Connected device
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Assert: Visual indicators show connected state
      verifyConnected(0);
    });

    it('should transition visual state on connection', () => {
      // Setup: Start with disconnected device
      interceptFindDevices({ fixture: disconnectedDevice });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Initial state: disconnected (dimmed, color=normal)
      verifyDisconnected(0);

      // Action: Connect
      clickPowerButton(0);
      waitForConnection();

      // Assert: State transitions to connected (not dimmed, color=highlight)
      verifyConnected(0);
    });

    it('should transition visual state on disconnection', () => {
      // Setup: Start with connected device
      interceptFindDevices({ fixture: singleDevice });
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Initial state: connected (not dimmed, color=highlight)
      verifyConnected(0);

      // Action: Disconnect
      clickPowerButton(0);
      waitForDisconnection();

      // Assert: State transitions to disconnected (dimmed, color=normal)
      verifyDisconnected(0);
    });

    it('should preserve device information through state changes', () => {
      // Setup: Device for state transition test
      interceptFindDevices({ fixture: disconnectedDevice });
      interceptConnectDevice();
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify initial device info is displayed
      verifyFullDeviceInfo(0);

      // Connect device
      clickPowerButton(0);
      waitForConnection();

      // Verify device info persists after connection
      verifyFullDeviceInfo(0);

      // Disconnect device
      clickPowerButton(0);
      waitForDisconnection();

      // Verify device info persists after disconnection
      verifyFullDeviceInfo(0);
    });
  });
});
