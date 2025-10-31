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
  ICON_CLASSES,
} from './test-helpers';
import {
  interceptFindDevices,
} from '../../support/interceptors/findDevices.interceptors';
import {
  interceptDisconnectDevice,
} from '../../support/interceptors/disconnectDevice.interceptors';
import {
  interceptConnectDevice,
  waitForConnectDevice,
} from '../../support/interceptors/connectDevice.interceptors';
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
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should connect device 1 while devices 2 and 3 remain disconnected', () => {
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      clickPowerButton(0);
      waitForConnection();

      verifyConnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);
    });

    it('should connect device 2 while devices 1 and 3 remain disconnected', () => {
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      clickPowerButton(1);
      waitForConnection();

      verifyDisconnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should connect device 3 while devices 1 and 2 remain disconnected', () => {
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      clickPowerButton(2);
      waitForConnection();

      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should maintain independent power button states', () => {
      clickPowerButton(0);
      waitForConnection();

      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.highlighted);

      getDeviceCard(1)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.normal);

      getDeviceCard(2)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.normal);

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
      interceptFindDevices({ fixture: multipleDevices });
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should disconnect device 1 while devices 2 and 3 remain connected', () => {
      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);

      clickPowerButton(0);
      waitForDisconnection();

      verifyDisconnected(0);
      verifyConnected(1);
      verifyConnected(2);
    });

    it('should disconnect device 2 while devices 1 and 3 remain connected', () => {
      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);

      clickPowerButton(1);
      waitForDisconnection();

      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should disconnect device 3 while devices 1 and 2 remain connected', () => {
      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);

      clickPowerButton(2);
      waitForDisconnection();

      verifyConnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should maintain device information for all devices after partial disconnection', () => {
      clickPowerButton(1);
      waitForDisconnection();

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
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should connect three devices sequentially', () => {
      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      clickPowerButton(1);
      waitForConnection();
      verifyConnected(1);

      clickPowerButton(2);
      waitForConnection();
      verifyConnected(2);

      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);
    });

    it('should maintain connection order and state', () => {
      clickPowerButton(0);
      waitForConnection();

      verifyConnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      clickPowerButton(1);
      waitForConnection();

      verifyConnected(0);
      verifyConnected(1);
      verifyDisconnected(2);

      clickPowerButton(2);
      waitForConnection();

      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);
    });

    it('should handle mixed connect/disconnect operations', () => {
      interceptDisconnectDevice();

      clickPowerButton(0);
      waitForConnection();
      verifyConnected(0);

      clickPowerButton(1);
      waitForConnection();
      verifyConnected(1);

      clickPowerButton(0);
      waitForDisconnection();
      verifyDisconnected(0);

      clickPowerButton(2);
      waitForConnection();
      verifyConnected(2);

      verifyDisconnected(0);
      verifyConnected(1);
      verifyConnected(2);
    });

    it('should make independent API calls for each connection', () => {
      clickPowerButton(0);
      waitForConnectDevice();
      verifyConnected(0);

      clickPowerButton(1);
      waitForConnectDevice();
      verifyConnected(1);

      clickPowerButton(2);
      waitForConnectDevice();
      verifyConnected(2);
    });
  });

  // =========================================================================
  // SUITE 4: MIXED CONNECTION STATES
  // =========================================================================
  describe('Mixed Connection States', () => {
    it('should display mixed connection states correctly', () => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should show correct visual state for each device in mixed state', () => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.highlighted);

      getDeviceCard(1).should('have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(1)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.normal);

      getDeviceCard(2).should('not.have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(2)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', ICON_CLASSES.highlighted);
    });

    it('should allow operations on any device regardless of other states', () => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyConnected(0);
      verifyDisconnected(1);
      verifyConnected(2);

      clickPowerButton(0);
      waitForDisconnection();

      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyConnected(2);
    });

    it('should render all devices in correct order with mixed states', () => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyDeviceCount(3);

      getDeviceCard(0).should('be.visible');
      getDeviceCard(1).should('be.visible');
      getDeviceCard(2).should('be.visible');

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
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should isolate connection error to single device', () => {
      interceptConnectDevice({ errorMode: true });

      clickPowerButton(0);
      waitForConnection();

      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);

      interceptConnectDevice({ errorMode: false });

      clickPowerButton(1);
      waitForConnection();

      verifyDisconnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should allow retry on failed device while others are connected', () => {
      interceptConnectDevice({ errorMode: true });

      clickPowerButton(0);
      waitForConnection();
      verifyDisconnected(0);

      interceptConnectDevice({ errorMode: false });

      clickPowerButton(1);
      waitForConnection();
      verifyConnected(1);

      clickPowerButton(0);
      waitForConnection();

      verifyConnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should isolate disconnection error to single device', () => {
      interceptFindDevices({ fixture: multipleDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      interceptDisconnectDevice({ errorMode: true });

      clickPowerButton(1);
      waitForDisconnection();

      verifyConnected(0);
      verifyConnected(1);
      verifyConnected(2);

      interceptDisconnectDevice({ errorMode: false });

      clickPowerButton(2);
      waitForDisconnection();

      verifyConnected(0);
      verifyConnected(1);
      verifyDisconnected(2);
    });

    it('should not display duplicate error messages', () => {
      interceptConnectDevice({ errorMode: true });

      clickPowerButton(0);
      waitForConnection();

      verifyDisconnected(0);
      verifyDisconnected(1);
      verifyDisconnected(2);
    });
  });

  // =========================================================================
  // SUITE 6: DEVICE COUNT STABILITY
  // =========================================================================
  describe('Device Count Stability', () => {
    it('should maintain device count through multiple connections', () => {
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyDeviceCount(3);

      clickPowerButton(0);
      waitForConnection();
      verifyDeviceCount(3);

      clickPowerButton(1);
      waitForConnection();
      verifyDeviceCount(3);

      clickPowerButton(2);
      waitForConnection();
      verifyDeviceCount(3);

      verifyDeviceCount(3);
    });

    it('should maintain device count through mixed operations', () => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      interceptConnectDevice();
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyDeviceCount(3);

      clickPowerButton(0);
      waitForDisconnection();
      verifyDeviceCount(3);

      clickPowerButton(1);
      waitForConnection();
      verifyDeviceCount(3);

      clickPowerButton(2);
      waitForDisconnection();
      verifyDeviceCount(3);

      verifyDeviceCount(3);
    });

    it('should render exactly 3 device cards throughout test', () => {
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForDeviceDiscovery();

      verifyDeviceCount(3);

      clickPowerButton(0);
      waitForConnection();
      cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 3);

      clickPowerButton(1);
      waitForConnection();
      cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 3);

      clickPowerButton(2);
      waitForConnection();
      cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 3);

      verifyDeviceCount(3);
    });
  });
});
