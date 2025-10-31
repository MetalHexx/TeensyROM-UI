/// <reference types="cypress" />

/**
 * Device Toolbar Button Disabled State Tests
 *
 * TDD Test Suite: Verify that action buttons are disabled when no connected devices are available.
 *
 * Scenarios:
 * 1. No devices connected (empty devices array)
 * 2. All devices disconnected (isConnected: false)
 * 3. At least one device connected (isConnected: true)
 *
 * The Refresh Devices button should remain always enabled in all scenarios.
 */

import {
  navigateToDeviceView,
  waitForDeviceDiscovery,
  DEVICE_CARD_SELECTORS,
  clickPowerButton,
  waitForConnection,
  waitForDisconnection,
  DEVICE_TOOLBAR_SELECTORS,
} from './test-helpers';
import {
  interceptFindDevices,
} from '../../support/interceptors/findDevices.interceptors';
import {
  interceptConnectDevice,
} from '../../support/interceptors/connectDevice.interceptors';
import {
  interceptDisconnectDevice,
} from '../../support/interceptors/disconnectDevice.interceptors';
import {
  singleDevice,
  noDevices,
  threeDisconnectedDevices,
  mixedConnectionDevices,
  disconnectedDevice,
} from '../../support/test-data/fixtures';

describe('Device Toolbar Button Disabled State', () => {
  // =========================================================================
  // SUITE 1: NO DEVICES (EMPTY STATE)
  // =========================================================================
  describe('No Devices Connected - Empty State', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: noDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should disable Index All button when no devices exist', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
        .should('be.disabled');
    });

    it('should disable Reset Devices button when no devices exist', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
        .should('be.disabled');
    });

    it('should disable Ping Devices button when no devices exist', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
        .should('be.disabled');
    });

    it('should keep Refresh Devices button enabled when no devices exist', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.refreshButton)
        .should('not.be.disabled');
    });
  });

  // =========================================================================
  // SUITE 2: ALL DEVICES DISCONNECTED
  // =========================================================================
  describe('All Devices Disconnected', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: threeDisconnectedDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should disable Index All button when all devices are disconnected', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
        .should('be.disabled');
    });

    it('should disable Reset Devices button when all devices are disconnected', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
        .should('be.disabled');
    });

    it('should disable Ping Devices button when all devices are disconnected', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
        .should('be.disabled');
    });

    it('should keep Refresh Devices button enabled when all devices are disconnected', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.refreshButton)
        .should('not.be.disabled');
    });

    it('should display three disconnected device cards', () => {
      cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 3);
    });
  });

  // =========================================================================
  // SUITE 3: AT LEAST ONE DEVICE CONNECTED
  // =========================================================================
  describe('At Least One Device Connected', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should enable Index All button when at least one device is connected', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
        .should('not.be.disabled');
    });

    it('should enable Reset Devices button when at least one device is connected', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
        .should('not.be.disabled');
    });

    it('should enable Ping Devices button when at least one device is connected', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
        .should('not.be.disabled');
    });

    it('should keep Refresh Devices button enabled when at least one device is connected', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.refreshButton)
        .should('not.be.disabled');
    });

    it('should display the connected device card', () => {
      cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 1);
    });
  });

  // =========================================================================
  // SUITE 4: MIXED CONNECTION STATES (SOME CONNECTED, SOME DISCONNECTED)
  // =========================================================================
  describe('Mixed Device Connection States', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: mixedConnectionDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should enable Index All button when at least one device is connected (mixed state)', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
        .should('not.be.disabled');
    });

    it('should enable Reset Devices button when at least one device is connected (mixed state)', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
        .should('not.be.disabled');
    });

    it('should enable Ping Devices button when at least one device is connected (mixed state)', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
        .should('not.be.disabled');
    });

    it('should keep Refresh Devices button enabled (mixed state)', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.refreshButton)
        .should('not.be.disabled');
    });

    it('should display all three devices regardless of connection state', () => {
      cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', 3);
    });

    it('should visually distinguish connected from disconnected devices', () => {
      cy.get(DEVICE_CARD_SELECTORS.card).eq(0).should('not.have.class', 'dimmed');
      cy.get(DEVICE_CARD_SELECTORS.card).eq(1).should('have.class', 'dimmed');
      cy.get(DEVICE_CARD_SELECTORS.card).eq(2).should('not.have.class', 'dimmed');
    });
  });

  // =========================================================================
  // SUITE 5: BUTTON ACCESSIBILITY AND VISIBILITY
  // =========================================================================
  describe('Button Accessibility and Visibility', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: noDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should render toolbar with all four buttons visible', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.container).should('be.visible');
      cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton).should('be.visible');
      cy.get(DEVICE_TOOLBAR_SELECTORS.refreshButton).should('be.visible');
      cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton).should('be.visible');
      cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton).should('be.visible');
    });

    it('should have proper button labels for disabled action buttons', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllContainer).should('contain.text', 'Index All');
      cy.get(DEVICE_TOOLBAR_SELECTORS.resetContainer).should('contain.text', 'Reset Devices');
      cy.get(DEVICE_TOOLBAR_SELECTORS.pingContainer).should('contain.text', 'Ping Devices');
    });

    it('should have proper button label for always-enabled Refresh button', () => {
      cy.get(DEVICE_TOOLBAR_SELECTORS.refreshContainer).should('contain.text', 'Refresh Devices');
    });
  });

  // =========================================================================
  // SUITE 6: FUNCTIONAL WORKFLOW - DEVICE CONNECTION STATE CHANGES
  // =========================================================================
  describe('Functional Workflow: Device Connection State Changes', () => {
    /**
     * Test Scenario: Start with a disconnected device, connect it, verify buttons enable,
     * then disconnect, and verify buttons disable again.
     *
     * This validates the complete workflow where button disabled state changes reactively
     * as device connection state changes.
     */
    describe('Single Device Connect/Disconnect Workflow', () => {
      beforeEach(() => {
        interceptFindDevices({ fixture: disconnectedDevice });
        interceptConnectDevice({ device: singleDevice.devices[0] });
        interceptDisconnectDevice();

        navigateToDeviceView();
        waitForDeviceDiscovery();
      });

      it('should start with action buttons disabled when device is disconnected', () => {
        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
          .should('be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
          .should('be.disabled');
      });

      it('should enable action buttons after connecting a device via power button', () => {
        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('be.disabled');

        clickPowerButton(0);
        waitForConnection();

        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('not.be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
          .should('not.be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
          .should('not.be.disabled');
      });

      it('should disable action buttons after disconnecting a connected device', () => {
        clickPowerButton(0);
        waitForConnection();

        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('not.be.disabled');

        clickPowerButton(0);
        waitForDisconnection();

        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
          .should('be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
          .should('be.disabled');
      });

      it('should keep Refresh Devices button enabled throughout connect/disconnect workflow', () => {
        cy.get(DEVICE_TOOLBAR_SELECTORS.refreshButton)
          .should('not.be.disabled');

        clickPowerButton(0);
        waitForConnection();

        cy.get(DEVICE_TOOLBAR_SELECTORS.refreshButton)
          .should('not.be.disabled');

        clickPowerButton(0);
        waitForDisconnection();

        cy.get(DEVICE_TOOLBAR_SELECTORS.refreshButton)
          .should('not.be.disabled');
      });
    });

    describe('Multiple Devices with Mixed States', () => {
      beforeEach(() => {
        interceptFindDevices({ fixture: threeDisconnectedDevices });
        interceptConnectDevice({ device: threeDisconnectedDevices.devices[0] });
        interceptDisconnectDevice();

        navigateToDeviceView();
        waitForDeviceDiscovery();
      });

      it('should start with all action buttons disabled when all three devices are disconnected', () => {
        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
          .should('be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
          .should('be.disabled');
      });

      it('should enable action buttons after connecting first of three devices', () => {
        clickPowerButton(0);
        waitForConnection();

        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('not.be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
          .should('not.be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
          .should('not.be.disabled');
      });

      it('should disable action buttons after disconnecting the only connected device', () => {
        clickPowerButton(0);
        waitForConnection();

        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('not.be.disabled');

        clickPowerButton(0);
        waitForDisconnection();

        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
          .should('be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
          .should('be.disabled');
      });
    });
  });
});
