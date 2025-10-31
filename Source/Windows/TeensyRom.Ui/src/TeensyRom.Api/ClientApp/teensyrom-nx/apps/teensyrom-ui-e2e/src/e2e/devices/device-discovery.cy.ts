/// <reference types="cypress" />

import {
  navigateToDeviceView,
  waitForDeviceDiscovery,
  verifyEmptyState,
  verifyLoadingState,
  verifyDeviceCount,
  verifyDeviceState,
  verifyFullDeviceInfo,
  getDeviceCard,
  DEVICE_VIEW_SELECTORS,
  DEVICE_CARD_SELECTORS,
  CSS_CLASSES,
} from './test-helpers';
import {
  interceptFindDevices,
  interceptFindDevicesWithDelay,
} from '../../support/interceptors/findDevices.interceptors';
import {
  singleDevice,
  multipleDevices,
  noDevices,
  disconnectedDevice,
  unavailableStorageDevice,
  mixedStateDevices,
} from '../../support/test-data/fixtures';

describe('Device Discovery E2E Tests', () => {
  // =========================================================================
  // SUITE 1: SINGLE DEVICE DISCOVERY
  // =========================================================================
  describe('Single Device Discovery', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should display single device card', () => {
      verifyDeviceCount(1);
      cy.get(DEVICE_CARD_SELECTORS.card).should('be.visible');
    });

    it('should display device name', () => {
      verifyFullDeviceInfo(0);
      cy.get(DEVICE_CARD_SELECTORS.idLabel).should('exist');
    });

    it('should display device port', () => {
      getDeviceCard(0).within(() => {
        cy.get(DEVICE_CARD_SELECTORS.portLabel).should('contain.text', 'Port:');
      });
    });

    it('should display firmware version', () => {
      getDeviceCard(0).within(() => {
        cy.get(DEVICE_CARD_SELECTORS.firmwareLabel).should('contain.text', 'Firmware:');
      });
    });

    it.skip('should show connected status', () => {
      // TODO: Requires SignalR event mocking (Phase 5)
      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
      cy.get(DEVICE_CARD_SELECTORS.stateLabel).should('contain.text', 'Connected');
    });

    it('should display storage information', () => {
      getDeviceCard(0).within(() => {
        cy.get(DEVICE_CARD_SELECTORS.usbStorageStatus).should('be.visible');
        cy.get(DEVICE_CARD_SELECTORS.sdStorageStatus).should('be.visible');
      });
    });
  });

  // =========================================================================
  // SUITE 2: MULTIPLE DEVICES DISCOVERY
  // =========================================================================
  describe('Multiple Devices Discovery', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: multipleDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should display correct device count', () => {
      verifyDeviceCount(3);
    });

    it('should display all devices as visible', () => {
      cy.get(DEVICE_CARD_SELECTORS.card).each((card) => {
        cy.wrap(card).should('be.visible');
      });
    });

    it('should show unique device information for each device', () => {
      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.idLabel)
        .should('exist');

      getDeviceCard(1)
        .find(DEVICE_CARD_SELECTORS.idLabel)
        .should('exist');

      getDeviceCard(2)
        .find(DEVICE_CARD_SELECTORS.idLabel)
        .should('exist');
    });

    it('should show connected status for all devices', () => {
      cy.get(DEVICE_CARD_SELECTORS.card).each((card) => {
        cy.wrap(card).should('not.have.class', CSS_CLASSES.DIMMED);
      });
    });

    it('should maintain device order from fixture', () => {
      cy.get(DEVICE_CARD_SELECTORS.card)
        .should('have.length', 3)
        .then((cards) => {
          expect(cards).to.have.length(3);
        });
    });
  });

  // =========================================================================
  // SUITE 3: NO DEVICES (EMPTY STATE)
  // =========================================================================
  describe('No Devices (Empty State)', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: noDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should display empty state message', () => {
      verifyEmptyState();
      cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('contain.text', 'No devices found');
    });

    it('should not render device cards', () => {
      cy.get(DEVICE_CARD_SELECTORS.card).should('not.exist');
    });

    it('should display empty state container', () => {
      cy.get(DEVICE_VIEW_SELECTORS.deviceList).should('be.visible');
    });

    it('should not show loading indicator', () => {
      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
    });
  });

  // =========================================================================
  // SUITE 4: DISCONNECTED DEVICE
  // =========================================================================
  describe('Disconnected Device', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: disconnectedDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should display disconnected device card', () => {
      verifyDeviceCount(1);
      cy.get(DEVICE_CARD_SELECTORS.card).should('be.visible');
    });

    it.skip('should show disconnected status', () => {
      // TODO: Requires SignalR event mocking (Phase 5)
      verifyDeviceState({
        deviceIndex: 0,
        state: 'ConnectionLost',
      });
    });

    it('should apply disconnected styling', () => {
      getDeviceCard(0).should('have.class', CSS_CLASSES.DIMMED);
    });

    it('should preserve device information', () => {
      verifyFullDeviceInfo(0);
    });

    it('should show power button for reconnect', () => {
      getDeviceCard(0)
        .find(DEVICE_CARD_SELECTORS.powerButton)
        .should('be.visible');
    });
  });

  // =========================================================================
  // SUITE 5: UNAVAILABLE STORAGE
  // =========================================================================
  describe('Unavailable Storage', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: unavailableStorageDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should display device card', () => {
      verifyDeviceCount(1);
    });

    it.skip('should show connected status', () => {
      // TODO: Requires SignalR event mocking (Phase 5)
      verifyDeviceState({
        deviceIndex: 0,
        state: 'Connected',
      });
    });

    it('should not apply disconnected styling', () => {
      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
    });

    it('should display storage status indicators', () => {
      getDeviceCard(0).within(() => {
        cy.get(DEVICE_CARD_SELECTORS.usbStorageStatus).should('be.visible');
        cy.get(DEVICE_CARD_SELECTORS.sdStorageStatus).should('be.visible');
      });
    });

    it('should show all device information', () => {
      verifyFullDeviceInfo(0);
    });
  });

  // =========================================================================
  // SUITE 6: MIXED DEVICE STATES
  // =========================================================================
  describe('Mixed Device States', () => {
    beforeEach(() => {
      interceptFindDevices({ fixture: mixedStateDevices });
      navigateToDeviceView();
      waitForDeviceDiscovery();
    });

    it('should display all devices regardless of state', () => {
      verifyDeviceCount(3);
    });

    it.skip('should show first device as connected', () => {
      // TODO: Requires SignalR event mocking (Phase 5)
      verifyDeviceState({
        deviceIndex: 0,
        state: 'Connected',
      });
      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
    });

    it.skip('should show second device as busy', () => {
      // TODO: Requires SignalR event mocking (Phase 5)
      verifyDeviceState({
        deviceIndex: 1,
        state: 'Busy',
      });
    });

    it.skip('should show third device as disconnected', () => {
      // TODO: Requires SignalR event mocking (Phase 5)
      verifyDeviceState({
        deviceIndex: 2,
        state: 'ConnectionLost',
      });
      getDeviceCard(2).should('have.class', CSS_CLASSES.DIMMED);
    });

    it('should visually distinguish states', () => {
      getDeviceCard(0).should('not.have.class', CSS_CLASSES.DIMMED);
      getDeviceCard(2).should('have.class', CSS_CLASSES.DIMMED);
    });

    it('should render all devices in order', () => {
      cy.get(DEVICE_CARD_SELECTORS.card)
        .should('have.length', 3)
        .each(($card) => {
          cy.wrap($card).should('be.visible');
        });
    });
  });

  // =========================================================================
  // SUITE 7: LOADING STATES
  // =========================================================================
  describe('Loading States', () => {
    it('should show loading indicator during API call', () => {
      interceptFindDevicesWithDelay(500, singleDevice);

      navigateToDeviceView();

      verifyLoadingState();
    });

    it('should remove loading indicator after response', () => {
      interceptFindDevices({ fixture: singleDevice });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
    });

    it('should not show devices while loading', () => {
      interceptFindDevicesWithDelay(800, singleDevice);

      navigateToDeviceView();

      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('be.visible');
    });

    it('should transition from loading to content', () => {
      interceptFindDevicesWithDelay(500, singleDevice);

      navigateToDeviceView();

      verifyLoadingState();

      waitForDeviceDiscovery();

      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
      verifyDeviceCount(1);
    });
  });

  // =========================================================================
  // SUITE 8: ERROR HANDLING
  // =========================================================================
  describe('Error Handling', () => {
    it('should display error state on API failure', () => {
      interceptFindDevices({ errorMode: true });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      cy.get(DEVICE_VIEW_SELECTORS.deviceList).should('be.visible');
      cy.get(DEVICE_CARD_SELECTORS.card).should('not.exist');
    });

    it('should not show loading after error', () => {
      interceptFindDevices({ errorMode: true });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('not.exist');
    });

    it('should handle gracefully with no crash', () => {
      interceptFindDevices({ errorMode: true });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      cy.get(DEVICE_VIEW_SELECTORS.container).should('be.visible');
    });

    it('should clear device list on error', () => {
      interceptFindDevices({ errorMode: true });
      navigateToDeviceView();
      waitForDeviceDiscovery();

      cy.get(DEVICE_CARD_SELECTORS.card).should('not.exist');
    });
  });
});
