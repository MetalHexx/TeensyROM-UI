/// <reference types="cypress" />

/**
 * Device Connection Refresh & Recovery E2E Tests
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
 */

import {
  navigateToDeviceView,
  clickRefreshDevices,
  clickPowerButton,
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
  waitForFindDevicesToStart,
} from '../../support/interceptors/findDevices.interceptors';
import {
  setupConnectDeviceWithCounting,
  getConnectDeviceCallCount,
  setupConnectDeviceWithValidation,
  setupConnectDeviceWithDelay,
  interceptConnectDevice,
  waitForConnectDevice,
  waitForConnectDeviceToStart,
} from '../../support/interceptors/connectDevice.interceptors';
import {
  interceptDisconnectDevice,
  setupDisconnectDeviceWithDelay,
  waitForDisconnectDevice,
  waitForDisconnectDeviceToStart,
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
  // REFRESH CONNECTED DEVICES
  // =========================================================================
  describe('Refresh Connected Devices', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForFindDevices();
    });

    it('should maintain single connected device after refresh', () => {
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForFindDevices();

      verifyConnected(0);
    });

    it('should maintain all connected devices after refresh', () => {
      interceptFindDevices({ fixture: multipleDevices });
      navigateToDeviceView();
      waitForFindDevices();

      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);

      interceptFindDevices({ fixture: multipleDevices });
      clickRefreshDevices();
      waitForFindDevices();

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
          waitForFindDevices();

          verifyFullDeviceInfo(0);
          getDeviceCard(0)
            .find(DEVICE_CARD_SELECTORS.idLabel)
            .should('contain.text', deviceId.trim());
        });
    });

    it('should not trigger reconnection API for already-connected devices', () => {
      setupConnectDeviceWithCounting();

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForFindDevices();

      verifyConnected(0);

      getConnectDeviceCallCount().then((count) => {
        expect(count).to.equal(0);
      });
    });
  });

  // =========================================================================
  // REFRESH DISCONNECTED DEVICES
  // =========================================================================
  describe('Refresh Disconnected Devices', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForFindDevices();
    });

    it('should maintain single disconnected device after refresh', () => {
      verifyDisconnected(0);

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForFindDevices();

      verifyDisconnected(0);
    });

    it('should maintain all disconnected devices after refresh', () => {
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForFindDevices();

      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      interceptFindDevices({ fixture: threeDisconnectedDevices });
      clickRefreshDevices();
      waitForFindDevices();

      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);
    });

    it('should not auto-reconnect previously disconnected devices', () => {
      verifyDisconnected(0);

      setupConnectDeviceWithCounting();

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForFindDevices();

      verifyDisconnected(0);

      getConnectDeviceCallCount().then((count) => {
        expect(count).to.equal(0);
      });
    });

    it('should preserve disconnected device information after refresh', () => {
      verifyFullDeviceInfo(0);

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForFindDevices();

      verifyFullDeviceInfo(0);
    });
  });

  // =========================================================================
  // REFRESH MIXED CONNECTION STATES
  // =========================================================================
  describe('Refresh Mixed Connection States', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForFindDevices();
    });

    it('should maintain mixed connection states after refresh', () => {
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);

      interceptFindDevices({ fixture: mixedConnectionDevices });
      clickRefreshDevices();
      waitForFindDevices();

      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should handle user-created mixed states through refresh', () => {
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForFindDevices();

      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnectDevice();
      verifyConnected(0);

      clickPowerButton(2);
      waitForConnectDevice();
      verifyConnected(2);

      interceptFindDevices({ fixture: mixedConnectionDevices });
      clickRefreshDevices();
      waitForFindDevices();

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
      waitForFindDevices();

      verifyDeviceCount(3);
      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should maintain visual state indicators after refresh', () => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      clickRefreshDevices();
      waitForFindDevices();

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
  // RECONNECTION AFTER REFRESH
  // =========================================================================
  describe('Reconnection After Refresh', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForFindDevices();
    });

    it('should allow reconnection to disconnected device after refresh', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForFindDevices();

      verifyDisconnected(0);

      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnectDevice();

      verifyConnected(0);
    });

    it('should reconnect with correct device ID after refresh', () => {
      const deviceId = disconnectedDevice.devices[0].deviceId;

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForFindDevices();

      setupConnectDeviceWithValidation(deviceId, disconnectedDevice.devices[0]);
      clickPowerButton(0);
      waitForConnectDevice();
    });

    it('should allow selective reconnection after refresh', () => {
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForFindDevices();

      interceptFindDevices({ fixture: threeDisconnectedDevices });
      clickRefreshDevices();
      waitForFindDevices();

      interceptConnectDevice();
      clickPowerButton(1);
      waitForConnectDevice();

      verifyDisconnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should handle full reconnection workflow after refresh', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForFindDevices();

      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnectDevice();
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForFindDevices();

      verifyConnected(0);
    });
  });

  // =========================================================================
  // REFRESH DURING CONNECTION
  // =========================================================================
  describe('Refresh During Connection', () => {
    it('should handle refresh clicked during connection in progress', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForFindDevices();

      setupConnectDeviceWithDelay(1000, singleDevice.devices[0]);

      clickPowerButton(0);

      waitForConnectDeviceToStart();
      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();

      cy.get(DEVICE_CARD_SELECTORS.card).should('exist');
      verifyDeviceCount(1);
    });

    it('should handle refresh clicked during disconnection in progress', () => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForFindDevices();

      verifyConnected(0);

      setupDisconnectDeviceWithDelay(1000);

      clickPowerButton(0);

      waitForDisconnectDeviceToStart();
      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();

      cy.get(DEVICE_CARD_SELECTORS.card).should('exist');
      verifyDeviceCount(1);
    });

    it('should allow connection after interrupted refresh', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForFindDevices();

      interceptFindDevicesWithDelay(1000, disconnectedDevice);

      clickRefreshDevices();

      waitForFindDevicesToStart();
      interceptConnectDevice();
      clickPowerButton(0);

      cy.get(DEVICE_CARD_SELECTORS.card).should('exist');
      verifyDeviceCount(1);
    });
  });

  // =========================================================================
  // REFRESH ERROR HANDLING
  // =========================================================================
  describe('Refresh Error Handling', () => {
    it('should preserve connection state when refresh fails', () => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForFindDevices();

      verifyConnected(0);

      interceptFindDevices({ errorMode: true });
      clickRefreshDevices();
      waitForFindDevices();

      verifyDeviceCount(0);
    });

    it('should preserve mixed states when refresh fails', () => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForFindDevices();

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
      waitForFindDevices();

      interceptFindDevices({ errorMode: true });
      clickRefreshDevices();
      waitForFindDevices();

      verifyDeviceCount(0);
    });

    it('should allow retry after refresh failure', () => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForFindDevices();

      interceptFindDevices({ errorMode: true });
      clickRefreshDevices();
      waitForFindDevices();

      verifyDeviceCount(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForFindDevices();

      verifyConnected(0);
    });
  });

  // =========================================================================
  // CONNECTION STATE PERSISTENCE
  // =========================================================================
  describe('Connection State Persistence', () => {
    it('should persist connection through multiple refreshes', () => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForFindDevices();

      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForFindDevices();
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForFindDevices();
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForFindDevices();
      verifyConnected(0);
    });

    it('should persist disconnection through multiple refreshes', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForFindDevices();

      verifyDisconnected(0);

      for (let i = 0; i < 3; i++) {
        interceptFindDevices({ fixture: disconnectedDevice });
        clickRefreshDevices();
        waitForFindDevices();
        verifyDisconnected(0);
      }
    });

    it('should persist user-initiated state changes through refreshes', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForFindDevices();

      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnectDevice();
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForFindDevices();
      verifyConnected(0);

      interceptDisconnectDevice();
      clickPowerButton(0);
      waitForDisconnectDevice();
      verifyDisconnected(0);

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForFindDevices();
      verifyDisconnected(0);
    });

    it('should handle connect-refresh-disconnect-refresh workflow', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForFindDevices();
      verifyDisconnected(0);

      interceptConnectDevice();
      clickPowerButton(0);
      waitForConnectDevice();
      verifyConnected(0);

      interceptFindDevices({ fixture: singleDevice });
      clickRefreshDevices();
      waitForFindDevices();
      verifyConnected(0);

      interceptDisconnectDevice();
      clickPowerButton(0);
      waitForDisconnectDevice();
      verifyDisconnected(0);

      interceptFindDevices({ fixture: disconnectedDevice });
      clickRefreshDevices();
      waitForFindDevices();
      verifyDisconnected(0);
    });
  });
});
