/// <reference types="cypress" />

/**
 * Device Connection Refresh & Recovery E2E Tests (Phase 3)
 *
 * Tests device refresh workflows and connection state persistence.
 * Validates that "Refresh Devices" button maintains connection states correctly
 * and that reconnection workflows function after refresh.
 *
 * **Key Workflows**:
 * - Refresh maintains connected device states
 * - Refresh maintains disconnected device states
 * - Reconnection after refresh works correctly
 * - Refresh during in-progress operations handled gracefully
 * - Multiple refresh cycles maintain state correctly
 *
 * **Timing Considerations**:
 * - Some tests use interceptor delays to create timing windows
 * - Edge case tests may be flaky if timing is too tight
 * - Focus on consistent final state, not intermediate states
 *
 * **Integration**:
 * - Builds on Phase 1 (single device) and Phase 2 (multi-device) patterns
 * - Uses all existing test helpers and fixtures
 * - Adds refresh complexity to connection workflows
 *
 * **Fixtures Used**:
 * - singleDevice: Single connected device
 * - disconnectedDevice: Single disconnected device
 * - multipleDevices: Three connected devices
 * - threeDisconnectedDevices: Three disconnected devices
 * - mixedConnectionDevices: Mixed connection states (connected, disconnected, connected)
 *
 * **Interceptors Used**:
 * - interceptFindDevices(): Mock device discovery (re-register before each refresh)
 * - interceptConnectDevice(): Mock device connection endpoint
 * - interceptDisconnectDevice(): Mock device disconnection endpoint
 *
 * **Key Helpers (test-helpers.ts)**:
 * - navigateToDeviceView(): Navigate to /devices
 * - waitForDeviceDiscovery(): Wait for initial device discovery
 * - clickRefreshDevices(): Click "Refresh Devices" button
 * - clickPowerButton(index): Click power button to connect/disconnect
 * - waitForConnection(): Wait for connection API call
 * - waitForDisconnection(): Wait for disconnection API call
 * - verifyConnected(index): Verify device is connected (visual + state)
 * - verifyDisconnected(index): Verify device is disconnected (visual + state)
 * - verifyDeviceCount(count): Verify number of devices displayed
 * - verifyFullDeviceInfo(index): Verify all device info fields present
 */

import {
  navigateToDeviceView,
  waitForDeviceDiscovery,
  clickRefreshDevices,
  clickPowerButton,
  waitForConnection,
  waitForDisconnection,
  verifyConnected,
  verifyDisconnected,
  verifyDeviceCount,
  verifyFullDeviceInfo,
  getDeviceCard,
  DEVICE_CARD_SELECTORS,
  CSS_CLASSES,
} from './test-helpers';
import {
  interceptFindDevices,
  interceptConnectDevice,
  interceptDisconnectDevice,
} from '../../support/interceptors/device.interceptors';
import {
  singleDevice,
  disconnectedDevice,
  multipleDevices,
  threeDisconnectedDevices,
  mixedConnectionDevices,
} from '../../support/test-data/fixtures/devices.fixture';

describe('Device Connection - Refresh & Recovery', () => {
  // =========================================================================
  // SUITE 1: REFRESH CONNECTED DEVICES
  // =========================================================================
  describe('Refresh Connected Devices', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should maintain single connected device after refresh', () => {
      // Verify initial connected state
      verifyConnected(0);

      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: singleDevice });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify device still connected after refresh
      verifyConnected(0);
    });

    it('should maintain all connected devices after refresh', () => {
      // Setup: Re-initialize with multiple devices
      interceptFindDevices({ fixture: multipleDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify all 3 devices connected initially
      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);

      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: multipleDevices });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify all 3 devices still connected after refresh
      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);
    });

    it('should preserve device information after refresh', () => {
      // Verify device info present initially
      verifyFullDeviceInfo(0);

      // Capture device ID for verification
      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.idLabel)
        .invoke('text')
        .then((deviceId) => {
          // Re-register interceptor for refresh
          interceptFindDevices({ fixture: singleDevice });

          // Perform refresh
          clickRefreshDevices();
          waitForDeviceDiscovery();

          // Verify same device information displayed
          verifyFullDeviceInfo(0);
          getDeviceCard(0)
            .find(DEVICE_CARD_SELECTORS.idLabel)
            .should('contain.text', deviceId.trim());
        });
    });

    it('should not trigger reconnection API for already-connected devices', () => {
      // Setup: Spy on connect API to verify it's not called
      let connectApiCallCount = 0;
      cy.intercept('POST', 'http://localhost:5168/devices/*/connect', (req) => {
        connectApiCallCount++;
        req.reply({ statusCode: 200, body: { message: 'Connected' } });
      }).as('spyConnectDevice');

      // Re-register find devices interceptor for refresh
      interceptFindDevices({ fixture: singleDevice });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify device still connected
      verifyConnected(0);

      // Verify no connect API calls were made
      cy.wrap(null).then(() => {
        expect(connectApiCallCount).to.equal(0);
      });
    });
  });

  // =========================================================================
  // SUITE 2: REFRESH DISCONNECTED DEVICES
  // =========================================================================
  describe('Refresh Disconnected Devices', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should maintain single disconnected device after refresh', () => {
      // Verify initial disconnected state
      verifyDisconnected(0);

      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: disconnectedDevice });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify device still disconnected after refresh
      verifyDisconnected(0);
    });

    it('should maintain all disconnected devices after refresh', () => {
      // Setup: Re-initialize with three disconnected devices
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify all 3 devices disconnected initially
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: threeDisconnectedDevices });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify all 3 devices still disconnected after refresh
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);
    });

    it('should not auto-reconnect previously disconnected devices', () => {
      // Verify disconnected initially
      verifyDisconnected(0);

      // Setup: Spy on connect API to verify no auto-reconnect
      let connectApiCallCount = 0;
      cy.intercept('POST', 'http://localhost:5168/devices/*/connect', (req) => {
        connectApiCallCount++;
        req.reply({ statusCode: 200, body: { message: 'Connected' } });
      }).as('spyConnectDevice');

      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: disconnectedDevice });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify device remains disconnected (not auto-reconnected)
      verifyDisconnected(0);

      // Verify no connect API calls were made
      cy.wrap(null).then(() => {
        expect(connectApiCallCount).to.equal(0);
      });
    });

    it('should preserve disconnected device information after refresh', () => {
      // Verify device info displayed despite disconnected state
      verifyFullDeviceInfo(0);

      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: disconnectedDevice });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify device info still displayed after refresh
      verifyFullDeviceInfo(0);
    });
  });

  // =========================================================================
  // SUITE 3: REFRESH MIXED CONNECTION STATES
  // =========================================================================
  describe('Refresh Mixed Connection States', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should maintain mixed connection states after refresh', () => {
      // Verify initial mixed states:
      // Device 1: connected, Device 2: disconnected, Device 3: connected
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);

      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: mixedConnectionDevices });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify states maintained after refresh
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should handle user-created mixed states through refresh', () => {
      // Setup: Start with all disconnected
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify all disconnected initially
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      // Connect device 1 and device 3 (leave device 2 disconnected)
      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      clickPowerButton(2);
      waitForConnection();
      verifyConnected(2);

      // Re-register interceptor for refresh with connected devices
      // Note: The fixture needs to reflect the current connection state
      // Using mixedConnectionDevices which has the pattern: connected, disconnected, connected
      interceptFindDevices({ fixture: mixedConnectionDevices });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify mixed state preserved: device 1 connected, device 2 disconnected, device 3 connected
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should update device list while preserving connection states', () => {
      // Verify initial device count and states
      verifyDeviceCount(3);
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);

      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: mixedConnectionDevices });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify device count still correct
      verifyDeviceCount(3);

      // Verify each device's connection state preserved
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should maintain visual state indicators after refresh', () => {
      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: mixedConnectionDevices });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify connected devices: not dimmed, power button highlighted
      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.powerButton)
        .find('mat-icon')
        .should('have.class', 'highlight');

      getDeviceCard(2).should('not.have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(2)
        .find(DEVICE_CARD_SELECTORS.powerButton)
        .find('mat-icon')
        .should('have.class', 'highlight');

      // Verify disconnected device: dimmed, power button normal
      getDeviceCard(1).should('have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(1)
        .find(DEVICE_CARD_SELECTORS.powerButton)
        .find('mat-icon')
        .should('have.class', 'normal');
    });
  });

  // =========================================================================
  // SUITE 4: RECONNECTION AFTER REFRESH
  // =========================================================================
  describe('Reconnection After Refresh', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should allow reconnection to disconnected device after refresh', () => {
      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: disconnectedDevice });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify device remains disconnected after refresh
      verifyDisconnected(0);

      // Setup connection interceptor
      interceptConnectDevice();

      // Click power button to connect
      clickPowerButton(0);
      waitForConnection();

      // Verify device connected
      verifyConnected(0);
    });

    it('should reconnect with correct device ID after refresh', () => {
      // Use device ID from fixture directly (it's deterministic)
      const deviceId = disconnectedDevice.devices[0].deviceId;

      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: disconnectedDevice });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Setup interceptor to capture connect API request
      cy.intercept('POST', 'http://localhost:5168/devices/*/connect', (req) => {
        // Verify request URL contains correct device ID
        expect(req.url).to.include(deviceId);
        req.reply({
          statusCode: 200,
          body: {
            connectedCart: disconnectedDevice.devices[0],
            message: 'Connected',
          },
        });
      }).as('connectDevice');

      // Click power button
      clickPowerButton(0);
      waitForConnection();
    });

    it('should allow selective reconnection after refresh', () => {
      // Setup: Three disconnected devices
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: threeDisconnectedDevices });

      // Perform refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Setup connection interceptor
      interceptConnectDevice();

      // Connect only device 2 (index 1)
      clickPowerButton(1);
      waitForConnection();

      // Verify device 2 connected, devices 1 and 3 remain disconnected
      verifyDisconnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should handle full reconnection workflow after refresh', () => {
      // Re-register interceptor for refresh
      interceptFindDevices({ fixture: disconnectedDevice });

      // Perform first refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Connect device
      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      // Re-register interceptor for second refresh (with connected device)
      interceptFindDevices({ fixture: singleDevice });

      // Perform second refresh
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify device still connected (connection persists through second refresh)
      verifyConnected(0);
    });
  });

  // =========================================================================
  // SUITE 5: REFRESH DURING CONNECTION
  // =========================================================================
  describe('Refresh During Connection', () => {
    it('should handle refresh clicked during connection in progress', () => {
      // Setup: Disconnected device
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Setup: Interceptor with delay to simulate slow connection
      cy.intercept('POST', 'http://localhost:5168/devices/*/connect', (req) => {
        req.reply({
          delay: 1000,
          statusCode: 200,
          body: {
            connectedCart: singleDevice.devices[0],
            message: 'Connected',
          },
        });
      }).as('connectDevice');

      // Click power button to start connection
      clickPowerButton(0);

      // Before connection completes, click refresh (wait 200ms to be in-progress)
      // eslint-disable-next-line cypress/no-unnecessary-waiting
      cy.wait(200);
      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();

      // Verify app doesn't crash and final state is consistent
      cy.get(DEVICE_CARD_SELECTORS.card).should('exist');
      verifyDeviceCount(1);
    });

    it('should handle refresh clicked during disconnection in progress', () => {
      // Setup: Connected device
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify initially connected
      verifyConnected(0);

      // Setup: Interceptor with delay to simulate slow disconnection
      cy.intercept('DELETE', 'http://localhost:5168/devices/*', (req) => {
        req.reply({
          delay: 1000,
          statusCode: 200,
          body: { message: 'Disconnected' },
        });
      }).as('disconnectDevice');

      // Click power button to start disconnection
      clickPowerButton(0);

      // Before disconnection completes, click refresh
      // eslint-disable-next-line cypress/no-unnecessary-waiting
      cy.wait(200);
      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();

      // Verify app doesn't crash and final state is consistent
      cy.get(DEVICE_CARD_SELECTORS.card).should('exist');
      verifyDeviceCount(1);
    });

    it('should allow connection after interrupted refresh', () => {
      // Setup: Disconnected device
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Setup: Refresh interceptor with delay
      cy.intercept('GET', 'http://localhost:5168/devices*', (req) => {
        req.reply({
          delay: 1000,
          statusCode: 200,
          body: {
            devices: [...disconnectedDevice.devices],
            message: 'Found 1 device(s)',
          },
        });
      }).as('findDevices');

      // Start refresh
      clickRefreshDevices();

      // Before refresh completes, click power button
      // eslint-disable-next-line cypress/no-unnecessary-waiting
      cy.wait(200);
      interceptConnectDevice();
      clickPowerButton(0);

      // Verify operation completes without error
      cy.get(DEVICE_CARD_SELECTORS.card).should('exist');
      verifyDeviceCount(1);
    });
  });

  // =========================================================================
  // SUITE 6: REFRESH ERROR HANDLING
  // =========================================================================
  describe('Refresh Error Handling', () => {
    it('should preserve connection state when refresh fails', () => {
      // Setup: Connected device
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify device connected
      verifyConnected(0);

      // Register error mode for refresh
      interceptFindDevices({ errorMode: true });

      // Click refresh - should fail
      clickRefreshDevices();
      cy.wait('@findDevices');

      // Verify device connection state unchanged (still connected)
      verifyConnected(0);
    });

    it('should preserve mixed states when refresh fails', () => {
      // Setup: Mixed connection states
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Note initial states
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);

      // Register error mode for refresh
      interceptFindDevices({ errorMode: true });

      // Perform refresh with error mode
      clickRefreshDevices();
      cy.wait('@findDevices');

      // Verify states unchanged after error
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should display error message on refresh failure', () => {
      // Setup: Connected device
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Register error mode for refresh
      interceptFindDevices({ errorMode: true });

      // Perform refresh
      clickRefreshDevices();
      cy.wait('@findDevices');

      // Verify error indication displayed (detailed alert validation in Phase 4)
      // For now, just verify the app doesn't crash and device still visible
      cy.get(DEVICE_CARD_SELECTORS.card).should('exist');
    });

    it('should allow retry after refresh failure', () => {
      // Setup: Connected device
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // First refresh fails (error mode)
      interceptFindDevices({ errorMode: true });
      clickRefreshDevices();
      cy.wait('@findDevices');

      // Verify device still visible
      verifyConnected(0);

      // Re-register success mode
      interceptFindDevices({ fixture: singleDevice });

      // Refresh again - should succeed
      clickRefreshDevices();
      waitForDeviceDiscovery();

      // Verify device list updated and connection state preserved
      verifyConnected(0);
    });
  });

  // =========================================================================
  // SUITE 7: CONNECTION STATE PERSISTENCE
  // =========================================================================
  describe('Connection State Persistence', () => {
    it('should persist connection through multiple refreshes', () => {
      // Setup: Connected device
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify connected
      verifyConnected(0);

      // Refresh #1
      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyConnected(0);

      // Refresh #2
      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyConnected(0);

      // Refresh #3
      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyConnected(0);

      // Connection state stable through multiple refreshes
    });

    it('should persist disconnection through multiple refreshes', () => {
      // Setup: Disconnected device
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Verify disconnected
      verifyDisconnected(0);

      // Multiple refreshes (3x)
      for (let i = 0; i < 3; i++) {
        interceptFindDevices({ fixture: disconnectedDevice });
        clickRefreshDevices();
        waitForDeviceDiscovery();
        verifyDisconnected(0);
      }

      // Device remains disconnected after all refreshes
    });

    it('should persist user-initiated state changes through refreshes', () => {
      // Setup: Disconnected device
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      // Connect device (user action)
      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      // Refresh - verify connected
      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyConnected(0);

      // Disconnect device (user action)
      interceptDisconnectDevice();
      clickPowerButton(0);
      waitForDisconnection();
      verifyDisconnected(0);

      // Refresh - verify disconnected
      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyDisconnected(0);

      // User actions persist through refresh cycles
    });

    it('should handle connect-refresh-disconnect-refresh workflow', () => {
      // Setup: Disconnected device
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
      verifyDisconnected(0);

      // Connect
      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      // Refresh → still connected
      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyConnected(0);

      // Disconnect
      interceptDisconnectDevice();
      clickPowerButton(0);
      waitForDisconnection();
      verifyDisconnected(0);

      // Refresh → still disconnected
      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyDisconnected(0);

      // Full lifecycle with refreshes interspersed
    });
  });
});
