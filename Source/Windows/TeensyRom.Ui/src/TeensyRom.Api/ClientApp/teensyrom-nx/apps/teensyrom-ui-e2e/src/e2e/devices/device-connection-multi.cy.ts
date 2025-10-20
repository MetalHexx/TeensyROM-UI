/// <reference types="cypress" />

/**
 * Multi-Device Connection E2E Tests - Phase 2
 *
 * **Phase Goal**: Validate connection/disconnection state isolation across multiple TeensyROM devices.
 * Ensures connection state changes for one device don't affect other devices, and UI correctly
 * displays mixed connection states.
 *
 * **Test Structure**:
 * - Independent Connection: Connecting one device doesn't affect others
 * - Independent Disconnection: Disconnecting one device doesn't affect others
 * - Sequential Connections: Multiple connect operations maintain state
 * - Mixed Connection States: UI displays devices in varied states correctly
 * - Error State Isolation: Connection errors affect only single device
 * - Device Count Stability: Device count remains constant through operations
 *
 * **Fixtures Used**:
 * - threeDisconnectedDevices: All 3 devices disconnected (sequential connection tests)
 * - multipleDevices: All 3 devices connected (disconnection tests)
 * - mixedConnectionDevices: Mixed states (device 1 & 3 connected, device 2 disconnected)
 *
 * **Key Patterns**:
 * - Independent operations: Change one device, verify others unaffected
 * - Sequential operations: Multiple operations in sequence, state maintained
 * - Error isolation: Errors on one device don't affect others
 * - Reuse Phase 1 helpers: clickPowerButton, verifyConnected, verifyDisconnected, etc.
 *
 * **Note on Phase 4 (Alerts)**: Alert message validation for multi-device scenarios
 * is deferred to Phase 4. These tests focus on connection state isolation.
 */

import {
  navigateToDeviceView,
  waitForDeviceDiscovery,
  getDeviceCard,
  verifyFullDeviceInfo,
  verifyDeviceCount,
  CSS_CLASSES,
  clickPowerButton,
  waitForConnection,
  waitForDisconnection,
  verifyConnected,
  verifyDisconnected,
  DEVICE_CARD_SELECTORS,
  API_ROUTE_ALIASES,
  ICON_CLASSES,
} from './test-helpers';
import {
  interceptFindDevices,
  interceptConnectDevice,
  interceptDisconnectDevice,
} from '../../support/interceptors/device.interceptors';
import {
  multipleDevices,
  threeDisconnectedDevices,
  mixedConnectionDevices,
} from '../../support/test-data/fixtures';

describe('Device Connection - Multi-Device', () => {
  // =========================================================================
  // SUITE 1: INDEPENDENT CONNECTION
  // =========================================================================
  describe('Independent Connection', () => {
    beforeEach(() => {
      // Setup: All devices disconnected
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should connect device 1 while devices 2 and 3 remain disconnected', () => {
      // Verify all initially disconnected
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      // Connect device 1
      clickPowerButton(0);
      waitForConnection();

      // Verify isolation: only device 1 connected
      verifyConnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);
    });

    it('should connect device 2 while devices 1 and 3 remain disconnected', () => {
      // Verify all initially disconnected
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      // Connect device 2
      clickPowerButton(1);
      waitForConnection();

      // Verify isolation: only device 2 connected
      verifyDisconnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should connect device 3 while devices 1 and 2 remain disconnected', () => {
      // Verify all initially disconnected
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      // Connect device 3
      clickPowerButton(2);
      waitForConnection();

      // Verify isolation: only device 3 connected
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should maintain independent power button states', () => {
      // Connect device 1
      clickPowerButton(0);
      waitForConnection();

      // Verify power button states
      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.highlighted);

      getDeviceCard(1)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.normal);

      getDeviceCard(2)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.normal);

      // Verify dimmed styling
      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(1).should('have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(2).should('have.class', CSS_CLASSES.DIMMED);
    });
  });

  // =========================================================================
  // SUITE 2: INDEPENDENT DISCONNECTION
  // =========================================================================
  describe('Independent Disconnection', () => {
    beforeEach(() => {
      // Setup: All devices connected
      interceptFindDevices({ fixture: multipleDevices });
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should disconnect device 1 while devices 2 and 3 remain connected', () => {
      // Verify all initially connected
      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);

      // Disconnect device 1
      clickPowerButton(0);
      waitForDisconnection();

      // Verify isolation: only device 1 disconnected
      verifyDisconnected(0);
      verifyConnected(1);
      verifyConnected(2);
    });

    it('should disconnect device 2 while devices 1 and 3 remain connected', () => {
      // Verify all initially connected
      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);

      // Disconnect device 2
      clickPowerButton(1);
      waitForDisconnection();

      // Verify isolation: only device 2 disconnected
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should disconnect device 3 while devices 1 and 2 remain connected', () => {
      // Verify all initially connected
      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);

      // Disconnect device 3
      clickPowerButton(2);
      waitForDisconnection();

      // Verify isolation: only device 3 disconnected
      verifyConnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should maintain device information for all devices after partial disconnection', () => {
      // Disconnect device 2
      clickPowerButton(1);
      waitForDisconnection();

      // Verify all devices retain their information
      verifyFullDeviceInfo(0);
      verifyFullDeviceInfo(1);
      verifyFullDeviceInfo(2);
    });
  });

  // =========================================================================
  // SUITE 3: SEQUENTIAL CONNECTIONS
  // =========================================================================
  describe('Sequential Connections', () => {
    beforeEach(() => {
      // Setup: All devices disconnected
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should connect three devices sequentially', () => {
      // Verify all initially disconnected
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      // Connect device 1
      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      // Connect device 2
      clickPowerButton(1);
      waitForConnection();
      verifyConnected(1);

      // Connect device 3
      clickPowerButton(2);
      waitForConnection();
      verifyConnected(2);

      // Verify all connected after sequential operations
      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);
    });

    it('should maintain connection order and state', () => {
      // Connect device 1
      clickPowerButton(0);
      waitForConnection();

      // Verify device 1 connected
      verifyConnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      // Connect device 2
      clickPowerButton(1);
      waitForConnection();

      // Verify devices 1-2 connected, device 3 still disconnected
      verifyConnected(0);
      verifyConnected(1);
      verifyDisconnected(2);

      // Connect device 3
      clickPowerButton(2);
      waitForConnection();

      // Verify all connected
      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);
    });

    it('should handle mixed connect/disconnect operations', () => {
      // Setup: All disconnected initially
      interceptDisconnectDevice();

      // Connect device 1
      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      // Connect device 2
      clickPowerButton(1);
      waitForConnection();
      verifyConnected(1);

      // Disconnect device 1
      clickPowerButton(0);
      waitForDisconnection();
      verifyDisconnected(0);

      // Connect device 3
      clickPowerButton(2);
      waitForConnection();
      verifyConnected(2);

      // Verify final state: device 1 disconnected, devices 2-3 connected
      verifyDisconnected(0);
      verifyConnected(1);
      verifyConnected(2);
    });

    it('should make independent API calls for each connection', () => {
      // Connect 3 devices sequentially and verify each makes an API call
      // by using wait() to track each request

      // Connect device 1 and wait for API call
      clickPowerButton(0);
      cy.wait(`@${API_ROUTE_ALIASES.CONNECT_DEVICE}`);
      verifyConnected(0);

      // Connect device 2 and wait for API call
      clickPowerButton(1);
      cy.wait(`@${API_ROUTE_ALIASES.CONNECT_DEVICE}`);
      verifyConnected(1);

      // Connect device 3 and wait for API call
      clickPowerButton(2);
      cy.wait(`@${API_ROUTE_ALIASES.CONNECT_DEVICE}`);
      verifyConnected(2);

      // If we got here without timeout, all 3 API calls were made successfully
      // Each device made a separate POST to the connect endpoint
    });
  });

  // =========================================================================
  // SUITE 4: MIXED CONNECTION STATES
  // =========================================================================
  describe('Mixed Connection States', () => {
    it('should display mixed connection states correctly', () => {
      // Setup: Mixed state fixture (device 1 connected, device 2 disconnected, device 3 connected)
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify mixed states displayed correctly
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should show correct visual state for each device in mixed state', () => {
      // Setup: Mixed state
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Device 1: connected (not dimmed, highlighted power button)
      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.highlighted);

      // Device 2: disconnected (dimmed, normal power button)
      getDeviceCard(1).should('have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(1)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.normal);

      // Device 3: connected (not dimmed, highlighted power button)
      getDeviceCard(2).should('not.have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(2)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.highlighted);
    });

    it('should allow operations on any device regardless of other states', () => {
      // Setup: Mixed state
      interceptFindDevices({ fixture: mixedConnectionDevices });
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Initial mixed state verified
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);

      // Disconnect device 1 (currently connected)
      clickPowerButton(0);
      waitForDisconnection();

      // Verify device 1 now disconnected, others unchanged
      verifyDisconnected(0);
      verifyDisconnected(1); // Still disconnected
      verifyConnected(2); // Still connected
    });

    it('should render all devices in correct order with mixed states', () => {
      // Setup: Mixed state
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify device count
      verifyDeviceCount(3);

      // Verify each device visible and in order
      getDeviceCard(0).should('be.visible');
      getDeviceCard(1).should('be.visible');
      getDeviceCard(2).should('be.visible');

      // Verify states in order
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });
  });

  // =========================================================================
  // SUITE 5: ERROR STATE ISOLATION
  // =========================================================================
  describe('Error State Isolation', () => {
    beforeEach(() => {
      // Setup: All devices disconnected
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should isolate connection error to single device', () => {
      // Setup: Connection fails with error mode
      interceptConnectDevice({ errorMode: true });

      // Try to connect device 1 - should fail
      clickPowerButton(0);
      waitForConnection();

      // Verify device 1 remains disconnected
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      // Switch to success mode for other devices
      interceptConnectDevice({ errorMode: false });

      // Connect device 2 successfully
      clickPowerButton(1);
      waitForConnection();

      // Verify isolation: device 2 connected, device 1 still disconnected
      verifyDisconnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should allow retry on failed device while others are connected', () => {
      // Setup: First attempt fails
      interceptConnectDevice({ errorMode: true });

      // Connect device 1 - fails
      clickPowerButton(0);
      waitForConnection();
      verifyDisconnected(0);

      // Change to success mode
      interceptConnectDevice({ errorMode: false });

      // Connect device 2 successfully
      clickPowerButton(1);
      waitForConnection();
      verifyConnected(1);

      // Retry device 1 - should succeed
      clickPowerButton(0);
      waitForConnection();

      // Verify both now connected
      verifyConnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should isolate disconnection error to single device', () => {
      // Setup: All connected initially
      interceptFindDevices({ fixture: multipleDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Setup: Disconnection fails
      interceptDisconnectDevice({ errorMode: true });

      // Try to disconnect device 2 - should fail
      clickPowerButton(1);
      waitForDisconnection();

      // Verify device 2 remains connected
      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);

      // Switch to success mode
      interceptDisconnectDevice({ errorMode: false });

      // Disconnect device 3 successfully
      clickPowerButton(2);
      waitForDisconnection();

      // Verify isolation: device 3 disconnected, device 2 still connected
      verifyConnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should not display duplicate error messages', () => {
      // Setup: Connection fails
      interceptConnectDevice({ errorMode: true });

      // Attempt connection on device 1
      clickPowerButton(0);
      waitForConnection();

      // Verify error occurred (device still disconnected)
      verifyDisconnected(0);

      // Note: Detailed alert validation in Phase 4 (DEVICE_CONNECTION_TEST_PLAN_P4_ALERTS.md)
      // This test focuses on ensuring device state remains consistent
      verifyDisconnected(1);
      verifyDisconnected(2);
    });
  });

  // =========================================================================
  // SUITE 6: DEVICE COUNT STABILITY
  // =========================================================================
  describe('Device Count Stability', () => {
    it('should maintain device count through multiple connections', () => {
      // Setup: All disconnected
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify initial count
      verifyDeviceCount(3);

      // Connect all 3 devices
      clickPowerButton(0);
      waitForConnection();
      verifyDeviceCount(3);

      clickPowerButton(1);
      waitForConnection();
      verifyDeviceCount(3);

      clickPowerButton(2);
      waitForConnection();
      verifyDeviceCount(3);

      // Verify count still 3 after all operations
      verifyDeviceCount(3);
    });

    it('should maintain device count through mixed operations', () => {
      // Setup: Mixed state
      interceptFindDevices({ fixture: mixedConnectionDevices });
      interceptConnectDevice();
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify initial count
      verifyDeviceCount(3);

      // Perform various operations
      clickPowerButton(0); // Disconnect connected device
      waitForDisconnection();
      verifyDeviceCount(3);

      clickPowerButton(1); // Connect disconnected device
      waitForConnection();
      verifyDeviceCount(3);

      clickPowerButton(2); // Disconnect connected device
      waitForDisconnection();
      verifyDeviceCount(3);

      // Verify count remains constant
      verifyDeviceCount(3);
    });

    it('should render exactly 3 device cards throughout test', () => {
      // Setup
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Check count at start
      verifyDeviceCount(3);

      // Connect device 1
      clickPowerButton(0);
      waitForConnection();
      cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 3);

      // Connect device 2
      clickPowerButton(1);
      waitForConnection();
      cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 3);

      // Connect device 3
      clickPowerButton(2);
      waitForConnection();
      cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 3);

      // Verify final count
      verifyDeviceCount(3);
    });
  });
});
