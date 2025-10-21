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
  interceptConnectDevice,
  interceptDisconnectDevice,
} from '../../support/interceptors/device.interceptors';
import {
  singleDevice,
  noDevices,
  threeDisconnectedDevices,
  mixedConnectionDevices,
  disconnectedDevice,
} from '../../support/test-data/fixtures/devices.fixture';

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
      // Device 1: Connected, Device 2: Disconnected, Device 3: Connected
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
      // First device should be connected (not dimmed)
      cy.get(DEVICE_CARD_SELECTORS.card).eq(0).should('not.have.class', 'dimmed');
      
      // Second device should be disconnected (dimmed)
      cy.get(DEVICE_CARD_SELECTORS.card).eq(1).should('have.class', 'dimmed');
      
      // Third device should be connected (not dimmed)
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
        // Start with one disconnected device
        interceptFindDevices({ fixture: disconnectedDevice });
        // Set up interceptors for connect/disconnect actions
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
        // Initial state: disconnected and buttons disabled
        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('be.disabled');

        // Click power button to connect device (first device at index 0)
        clickPowerButton(0);
        
        // Wait for connection API call
        waitForConnection();

        // After connection succeeds, the device store should update
        // and buttons should become enabled
        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('not.be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
          .should('not.be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
          .should('not.be.disabled');
      });

      it('should disable action buttons after disconnecting a connected device', () => {
        // Start by connecting the device
        clickPowerButton(0);
        waitForConnection();

        // Verify buttons are now enabled after connection
        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('not.be.disabled');

        // Now disconnect by clicking power button again
        clickPowerButton(0);
        waitForDisconnection();

        // After disconnection, buttons should be disabled again
        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
          .should('be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
          .should('be.disabled');
      });

      it('should keep Refresh Devices button enabled throughout connect/disconnect workflow', () => {
        // Refresh should be enabled initially (disconnected state)
        cy.get(DEVICE_TOOLBAR_SELECTORS.refreshButton)
          .should('not.be.disabled');

        // Connect device
        clickPowerButton(0);
        waitForConnection();

        // Refresh should still be enabled (connected state)
        cy.get(DEVICE_TOOLBAR_SELECTORS.refreshButton)
          .should('not.be.disabled');

        // Disconnect device
        clickPowerButton(0);
        waitForDisconnection();

        // Refresh should still be enabled (disconnected state)
        cy.get(DEVICE_TOOLBAR_SELECTORS.refreshButton)
          .should('not.be.disabled');
      });
    });

    /**
     * Test Scenario: Multiple devices with mixed connection states.
     * Connect one device to enable buttons, then disconnect it to disable buttons again.
     */
    describe('Multiple Devices with Mixed States', () => {
      beforeEach(() => {
        // Start with three disconnected devices
        interceptFindDevices({ fixture: threeDisconnectedDevices });
        // Set up interceptors for connect/disconnect
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
        // Connect the first device
        clickPowerButton(0);
        waitForConnection();

        // Now buttons should be enabled (at least one device is connected)
        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('not.be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.resetButton)
          .should('not.be.disabled');
        cy.get(DEVICE_TOOLBAR_SELECTORS.pingButton)
          .should('not.be.disabled');
      });

      it('should disable action buttons after disconnecting the only connected device', () => {
        // First, connect the device
        clickPowerButton(0);
        waitForConnection();

        // Verify buttons are enabled
        cy.get(DEVICE_TOOLBAR_SELECTORS.indexAllButton)
          .should('not.be.disabled');

        // Disconnect the device (it was the only connected one)
        clickPowerButton(0);
        waitForDisconnection();

        // Since all devices are now disconnected, buttons should be disabled again
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
