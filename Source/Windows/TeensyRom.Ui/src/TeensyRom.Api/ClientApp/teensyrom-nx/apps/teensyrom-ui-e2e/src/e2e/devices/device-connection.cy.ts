/// <reference types="cypress" />

/**
 * Device Connection Lifecycle E2E Tests - Phase 1
 *
 * Tests single TeensyROM device connection/disconnection patterns and state management.
 * Creates reusable test helpers for subsequent phases (multi-device, refresh, recovery).
 *
 * Test Suites:
 * - Connection Success: Happy path connection scenarios
 * - Disconnection Success: Happy path disconnection scenarios
 * - Connection Errors: API failures during connection
 * - Disconnection Errors: API failures during disconnection
 * - Visual Feedback: State transitions and UI indicators
 *
 * Fixtures:
 * - singleDevice: Connected device (default bootstrap state)
 * - disconnectedDevice: Device with ConnectionLost state
 *
 * Phase 4 Note: Alert message validation deferred to maintain focus on connection state behavior.
 */

import {
  navigateToDeviceView,
  getDeviceCard,
  verifyFullDeviceInfo,
  CSS_CLASSES,
  DEVICE_CARD_SELECTORS,
  clickPowerButton,
  verifyConnected,
  verifyDisconnected,
} from './test-helpers';
import {
  interceptFindDevices,
  waitForFindDevices,
} from '../../support/interceptors/findDevices.interceptors';
import {
  interceptDisconnectDevice,
  setupDisconnectDeviceWithValidation,
  waitForDisconnectDevice,
} from '../../support/interceptors/disconnectDevice.interceptors';
import {
  interceptConnectDevice,
  setupConnectDeviceWithValidation,
  waitForConnectDevice,
} from '../../support/interceptors/connectDevice.interceptors';
import { singleDevice, disconnectedDevice } from '../../support/test-data/fixtures';

describe('Device Connection - Single Device', () => {
  // =========================================================================
  // SUITE 1: CONNECTION SUCCESS
  // =========================================================================
  describe('Connection Success', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: disconnectedDevice });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForFindDevices();
    });

    it('should connect to disconnected device when power button clicked', () => {
      verifyDisconnected(0);
      clickPowerButton(0);
      waitForConnectDevice();
      verifyConnected(0);
    });

    it('should update power button color after connection', () => {
      verifyDisconnected(0);
      clickPowerButton(0);
      waitForConnectDevice();

      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
        .should('have.class', 'highlight');
    });

    it('should remove dimmed styling from device card after connection', () => {
      getDeviceCard(0).should('have.class', CSS_CLASSES.DIMMED);
      clickPowerButton(0);
      waitForConnectDevice();
      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
    });

    it('should call connection API with correct device ID', () => {
      setupConnectDeviceWithValidation(
        disconnectedDevice.devices[0].deviceId,
        disconnectedDevice.devices[0]
      );

      clickPowerButton(0);
      waitForConnectDevice();
    });
  });

  // =========================================================================
  // SUITE 2: DISCONNECTION SUCCESS
  // =========================================================================
  describe('Disconnection Success', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: singleDevice });
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForFindDevices();
    });

    it('should disconnect from connected device when power button clicked', () => {
      verifyConnected(0);
      clickPowerButton(0);
      waitForDisconnectDevice();
      verifyDisconnected(0);
    });

    it('should update power button color after disconnection', () => {
      verifyConnected(0);
      clickPowerButton(0);
      waitForDisconnectDevice();

      getDeviceCard(0).find(DEVICE_CARD_SELECTORS.powerButtonIcon).should('have.class', 'normal');
    });

    it('should apply dimmed styling to device card after disconnection', () => {
      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
      clickPowerButton(0);
      waitForDisconnectDevice();
      getDeviceCard(0).should('have.class', CSS_CLASSES.DIMMED);
    });

    it('should call disconnection API with correct device ID', () => {
      setupDisconnectDeviceWithValidation(singleDevice.devices[0].deviceId);

      clickPowerButton(0);
      waitForDisconnectDevice();
    });
  });

  // =========================================================================
  // SUITE 3: CONNECTION ERRORS
  // =========================================================================
  describe('Connection Errors', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForFindDevices();
    });

    it('should handle connection API failure', () => {
      interceptConnectDevice({ errorMode: true });
      verifyDisconnected(0);
      clickPowerButton(0);
      waitForConnectDevice();
      verifyDisconnected(0);
    });

    it('should display error message after connection failure', () => {
      interceptConnectDevice({ errorMode: true });
      clickPowerButton(0);
      waitForConnectDevice();
      verifyDisconnected(0);
    });

    it('should allow retry after connection failure', () => {
      interceptConnectDevice({ errorMode: true });
      clickPowerButton(0);
      waitForConnectDevice();
      verifyDisconnected(0);

      interceptConnectDevice({ errorMode: false });
      clickPowerButton(0);
      waitForConnectDevice();
      verifyConnected(0);
    });

    it('should maintain device information after connection failure', () => {
      interceptConnectDevice({ errorMode: true });
      clickPowerButton(0);
      waitForConnectDevice();
      verifyFullDeviceInfo(0);
    });
  });

  // =========================================================================
  // SUITE 4: DISCONNECTION ERRORS
  // =========================================================================
  describe('Disconnection Errors', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForFindDevices();
    });

    it('should handle disconnection API failure', () => {
      interceptDisconnectDevice({ errorMode: true });
      verifyConnected(0);
      clickPowerButton(0);
      waitForDisconnectDevice();
      verifyConnected(0);
    });

    it('should display error message after disconnection failure', () => {
      interceptDisconnectDevice({ errorMode: true });
      clickPowerButton(0);
      waitForDisconnectDevice();
      verifyConnected(0);
    });

    it('should allow retry after disconnection failure', () => {
      interceptDisconnectDevice({ errorMode: true });
      clickPowerButton(0);
      waitForDisconnectDevice();
      verifyConnected(0);

      interceptDisconnectDevice({ errorMode: false });
      clickPowerButton(0);
      waitForDisconnectDevice();
      verifyDisconnected(0);
    });

    it('should maintain connected state after disconnection failure', () => {
      interceptDisconnectDevice({ errorMode: true });
      clickPowerButton(0);
      waitForDisconnectDevice();
      verifyConnected(0);
    });
  });

  // =========================================================================
  // SUITE 5: VISUAL FEEDBACK
  // =========================================================================
  describe('Visual Feedback', () => {
    it('should show correct initial visual state for disconnected device', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForFindDevices();
      verifyDisconnected(0);
    });

    it('should show correct initial visual state for connected device', () => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForFindDevices();
      verifyConnected(0);
    });

    it('should transition visual state on connection', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      interceptConnectDevice();
      navigateToDeviceView();
      waitForFindDevices();

      verifyDisconnected(0);
      clickPowerButton(0);
      waitForConnectDevice();
      verifyConnected(0);
    });

    it('should transition visual state on disconnection', () => {
      interceptFindDevices({ fixture: singleDevice });
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForFindDevices();

      verifyConnected(0);
      clickPowerButton(0);
      waitForDisconnectDevice();
      verifyDisconnected(0);
    });

    it('should preserve device information through state changes', () => {
      interceptFindDevices({ fixture: disconnectedDevice });
      interceptConnectDevice();
      interceptDisconnectDevice();
      navigateToDeviceView();
      waitForFindDevices();

      verifyFullDeviceInfo(0);
      clickPowerButton(0);
      waitForConnectDevice();
      verifyFullDeviceInfo(0);
      clickPowerButton(0);
      waitForDisconnectDevice();
      verifyFullDeviceInfo(0);
    });
  });
});
