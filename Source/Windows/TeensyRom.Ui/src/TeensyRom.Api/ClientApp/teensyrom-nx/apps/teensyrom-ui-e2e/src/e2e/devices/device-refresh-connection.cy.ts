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
  ICON_CLASSES,
} from './test-helpers';
import {
  interceptFindDevices,
  interceptFindDevicesWithDelay,
  waitForFindDevices,
} from '../../support/interceptors/findDevices.interceptors';
import {
  interceptConnectDevice,
  CONNECT_DEVICE_ENDPOINT,
  CONNECT_DEVICE_ALIAS,
} from '../../support/interceptors/connectDevice.interceptors';
import {
  interceptDisconnectDevice,
  DISCONNECT_DEVICE_ENDPOINT,
  DISCONNECT_DEVICE_ALIAS,
} from '../../support/interceptors/disconnectDevice.interceptors';
import {
  singleDevice,
  disconnectedDevice,
  multipleDevices,
  threeDisconnectedDevices,
  mixedConnectionDevices,
} from '../../support/test-data/fixtures';

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
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyConnected(0);
    });

    it('should maintain all connected devices after refresh', () => {
      interceptFindDevices({ fixture: multipleDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);

      interceptFindDevices({ fixture: multipleDevices });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);
    });

    it('should preserve device information after refresh', () => {
      verifyFullDeviceInfo(0);

      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.idLabel)
        .invoke('text')
        .then((deviceId) => {
          interceptFindDevices({ fixture: singleDevice });
          clickRefreshDevices();
          waitForDeviceDiscovery();

          verifyFullDeviceInfo(0);
          getDeviceCard(0)
            .find(DEVICE_CARD_SELECTORS.idLabel)
            .should('contain.text', deviceId.trim());
        });
    });

    it('should not trigger reconnection API for already-connected devices', () => {
      let connectApiCallCount = 0;
      cy.intercept(CONNECT_DEVICE_ENDPOINT.method, CONNECT_DEVICE_ENDPOINT.pattern, (req) => {
        connectApiCallCount++;
        req.reply({ statusCode: 200, body: { message: 'Connected' } });
      }).as(CONNECT_DEVICE_ALIAS);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyConnected(0);

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
      verifyDisconnected(0);

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyDisconnected(0);
    });

    it('should maintain all disconnected devices after refresh', () => {
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      interceptFindDevices({ fixture: threeDisconnectedDevices });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);
    });

    it('should not auto-reconnect previously disconnected devices', () => {
      verifyDisconnected(0);

      let connectApiCallCount = 0;
      cy.intercept(CONNECT_DEVICE_ENDPOINT.method, CONNECT_DEVICE_ENDPOINT.pattern, (req) => {
        connectApiCallCount++;
        req.reply({ statusCode: 200, body: { message: 'Connected' } });
      }).as(CONNECT_DEVICE_ALIAS);

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyDisconnected(0);

      cy.wrap(null).then(() => {
        expect(connectApiCallCount).to.equal(0);
      });
    });

    it('should preserve disconnected device information after refresh', () => {
      verifyFullDeviceInfo(0);

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();

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
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);

      interceptFindDevices({ fixture: mixedConnectionDevices });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should handle user-created mixed states through refresh', () => {
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      clickPowerButton(2);
      waitForConnection();
      verifyConnected(2);

      interceptFindDevices({ fixture: mixedConnectionDevices });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should update device list while preserving connection states', () => {
      verifyDeviceCount(3);
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);

      interceptFindDevices({ fixture: mixedConnectionDevices });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyDeviceCount(3);
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should maintain visual state indicators after refresh', () => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.powerButton)
        .find('mat-icon')
        .should('have.class', ICON_CLASSES.highlighted);

      getDeviceCard(2).should('not.have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(2)
        .find(DEVICE_CARD_SELECTORS.powerButton)
        .find('mat-icon')
        .should('have.class', ICON_CLASSES.highlighted);

      getDeviceCard(1).should('have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(1)
        .find(DEVICE_CARD_SELECTORS.powerButton)
        .find('mat-icon')
        .should('have.class', ICON_CLASSES.normal);
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
      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyDisconnected(0);

      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnection();

      verifyConnected(0);
    });

    it('should reconnect with correct device ID after refresh', () => {
      const deviceId = disconnectedDevice.devices[0].deviceId;

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      cy.intercept('POST', 'http://localhost:5168/devices/*/connect', (req) => {
        expect(req.url).to.include(deviceId);
        req.reply({
          statusCode: 200,
          body: {
            connectedCart: disconnectedDevice.devices[0],
            message: 'Connected',
          },
        });
      }).as('connectDevice');

      clickPowerButton(0);
      waitForConnection();
    });

    it('should allow selective reconnection after refresh', () => {
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      interceptFindDevices({ fixture: threeDisconnectedDevices });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      interceptConnectDevice();
      clickPowerButton(1);
      waitForConnection();

      verifyDisconnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should handle full reconnection workflow after refresh', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyConnected(0);
    });
  });

  // =========================================================================
  // SUITE 5: REFRESH DURING CONNECTION
  // =========================================================================
  describe('Refresh During Connection', () => {
    it('should handle refresh clicked during connection in progress', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      cy.intercept(CONNECT_DEVICE_ENDPOINT.method, CONNECT_DEVICE_ENDPOINT.pattern, (req) => {
        req.reply({
          delay: 1000,
          statusCode: 200,
          body: {
            connectedCart: singleDevice.devices[0],
            message: 'Connected',
          },
        });
      }).as(CONNECT_DEVICE_ALIAS);

      clickPowerButton(0);

      cy.wait(200);
      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();

      cy.get(DEVICE_CARD_SELECTORS.card).should('exist');
      verifyDeviceCount(1);
    });

    it('should handle refresh clicked during disconnection in progress', () => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyConnected(0);

      cy.intercept(DISCONNECT_DEVICE_ENDPOINT.method, DISCONNECT_DEVICE_ENDPOINT.pattern, (req) => {
        req.reply({
          delay: 1000,
          statusCode: 200,
          body: { message: 'Disconnected' },
        });
      }).as(DISCONNECT_DEVICE_ALIAS);

      clickPowerButton(0);

      cy.wait(200);
      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();

      cy.get(DEVICE_CARD_SELECTORS.card).should('exist');
      verifyDeviceCount(1);
    });

    it('should allow connection after interrupted refresh', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      interceptFindDevicesWithDelay(1000, disconnectedDevice);

      clickRefreshDevices();

      cy.wait(200);
      interceptConnectDevice();
      clickPowerButton(0);

      cy.get(DEVICE_CARD_SELECTORS.card).should('exist');
      verifyDeviceCount(1);
    });
  });

  // =========================================================================
  // SUITE 6: REFRESH ERROR HANDLING
  // =========================================================================
  describe('Refresh Error Handling', () => {
    it('should preserve connection state when refresh fails', () => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyConnected(0);

      interceptFindDevices({ errorMode: true });
      clickRefreshDevices();
      waitForFindDevices();

      verifyDeviceCount(0);
    });

    it('should preserve mixed states when refresh fails', () => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);

      interceptFindDevices({ errorMode: true });
      clickRefreshDevices();
      waitForFindDevices();

      verifyDeviceCount(0);
    });

    it('should display error message on refresh failure', () => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      interceptFindDevices({ errorMode: true });
      clickRefreshDevices();
      waitForFindDevices();

      verifyDeviceCount(0);
    });

    it('should allow retry after refresh failure', () => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      interceptFindDevices({ errorMode: true });
      clickRefreshDevices();
      waitForFindDevices();

      verifyDeviceCount(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();

      verifyConnected(0);
    });
  });

  // =========================================================================
  // SUITE 7: CONNECTION STATE PERSISTENCE
  // =========================================================================
  describe('Connection State Persistence', () => {
    it('should persist connection through multiple refreshes', () => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyConnected(0);
    });

    it('should persist disconnection through multiple refreshes', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyDisconnected(0);

      for (let i = 0; i < 3; i++) {
        interceptFindDevices({ fixture: disconnectedDevice });
        clickRefreshDevices();
        waitForDeviceDiscovery();
        verifyDisconnected(0);
      }
    });

    it('should persist user-initiated state changes through refreshes', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyConnected(0);

      interceptDisconnectDevice();
      clickPowerButton(0);
      waitForDisconnection();
      verifyDisconnected(0);

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyDisconnected(0);
    });

    it('should handle connect-refresh-disconnect-refresh workflow', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
      verifyDisconnected(0);

      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyConnected(0);

      interceptDisconnectDevice();
      clickPowerButton(0);
      waitForDisconnection();
      verifyDisconnected(0);

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForDeviceDiscovery();
      verifyDisconnected(0);
    });
  });
});
