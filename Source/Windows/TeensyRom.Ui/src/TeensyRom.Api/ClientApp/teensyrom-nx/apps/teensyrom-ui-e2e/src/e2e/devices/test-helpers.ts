/// <reference types="cypress" />

/**
 * Device Discovery E2E Test Helpers & Selectors
 *
 * Centralized test utilities and data-testid selectors for device discovery tests.
 * This file contains reusable helper functions and a single source of truth for
 * all element selectors used across device discovery tests.
 */

// ============================================================================
// CENTRALIZED CONSTANTS (Single Source of Truth)
// ============================================================================

// API Routes & Endpoints
export const API_ROUTES = {
  BASE: '/api',
  DEVICES: '/api/devices',
  DEVICE_CONNECT: (deviceId: string) => `/api/devices/${deviceId}/connect`,
  DEVICE_DISCONNECT: (deviceId: string) => `/api/devices/${deviceId}`,
  DEVICE_PING: (deviceId: string) => `/api/devices/${deviceId}/ping`,
} as const;

// API Route Aliases (for cy.wait)
export const API_ROUTE_ALIASES = {
  FIND_DEVICES: 'findDevices',
  CONNECT_DEVICE: 'connectDevice',
  DISCONNECT_DEVICE: 'disconnectDevice',
  PING_DEVICE: 'pingDevice',
} as const;

// CSS Classes
export const CSS_CLASSES = {
  DIMMED: 'dimmed',
  UNAVAILABLE: 'unavailable',
  ERROR_MESSAGE: 'error-message',
} as const;

// DOM Attributes & Selectors
export const DOM_ATTRIBUTES = {
  ROLE_ALERT: 'alert',
  BODY_TAG: 'body',
  DATA_TESTID: 'data-testid',
} as const;

// String Constants
export const CONSTANTS = {
  ERROR_TEXT: 'error',
  DEFAULT_TIMEOUT: 5000,
} as const;

// ============================================================================
// CENTRALIZED SELECTORS (Single Source of Truth)
// ============================================================================
// These selectors are defined once and reused throughout tests to ensure
// consistency and make updates easy (change here, fix everywhere).

export const DEVICE_VIEW_SELECTORS = {
  container: '[data-testid="device-view"]',
  deviceList: '[data-testid="device-list"]',
  emptyStateMessage: '[data-testid="empty-state-message"]',
  loadingIndicator: '.busy-dialog-content',  // Bootstrap busy dialog (app initialization)
} as const;

export const DEVICE_CARD_SELECTORS = {
  card: '[data-testid="device-card"]',
  powerButton: '[data-testid="device-power-button"]',
  deviceInfo: '[data-testid="device-info"]',
  deviceStorage: '[data-testid="device-storage"]',
  // Device info labels
  idLabel: '[data-testid="device-id-label"]',
  firmwareLabel: '[data-testid="device-firmware-label"]',
  portLabel: '[data-testid="device-port-label"]',
  stateLabel: '[data-testid="device-state-label"]',
  compatibleLabel: '[data-testid="device-compatible-label"]',
  // Storage status
  usbStorageStatus: '[data-testid="usb-storage-status"]',
  sdStorageStatus: '[data-testid="sd-storage-status"]',
} as const;

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

/**
 * Navigate to the device view page
 */
export function navigateToDeviceView(): Cypress.Chainable<Cypress.AUTWindow> {
  return cy.visit('/devices', {
    onBeforeLoad: (win) => {
      // Clear storage to ensure fresh app state
      win.localStorage.clear();
      win.sessionStorage.clear();
    },
  });
}

/**
 * Wait for device discovery API call to complete
 * Uses extended timeout since app bootstrap triggers the call
 */
export function waitForDeviceDiscovery(timeout = 10000): void {
  cy.wait(`@${API_ROUTE_ALIASES.FIND_DEVICES}`, { timeout });
}

/**
 * Get a device card by its index (0-based)
 * Returns a chainable for fluent chaining with Cypress commands
 */
export function getDeviceCard(index: number): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(DEVICE_CARD_SELECTORS.card).eq(index);
}

/**
 * Verify device card is visible with expected information
 */
export function verifyDeviceCard(options: {
  index: number;
  name?: string;
  port?: string;
  shouldBeConnected?: boolean;
}): void {
  const { index, name, port, shouldBeConnected = true } = options;

  getDeviceCard(index).should('be.visible').within(() => {
    // Verify device name if provided
    if (name) {
      cy.get(DEVICE_CARD_SELECTORS.idLabel).should('contain.text', name);
    }

    // Verify port if provided
    if (port) {
      cy.get(DEVICE_CARD_SELECTORS.portLabel).should('contain.text', port);
    }

    // Verify connection status styling
    if (shouldBeConnected) {
      cy.get(DEVICE_CARD_SELECTORS.card).should('not.have.class', CSS_CLASSES.DIMMED);
    } else {
      cy.get(DEVICE_CARD_SELECTORS.card).should('have.class', CSS_CLASSES.DIMMED);
    }
  });
}

/**
 * Verify device count matches expected
 */
export function verifyDeviceCount(count: number): void {
  cy.get(DEVICE_VIEW_SELECTORS.deviceList).within(() => {
    if (count === 0) {
      cy.get(DEVICE_CARD_SELECTORS.card).should('not.exist');
    } else {
      cy.get(DEVICE_CARD_SELECTORS.card).should('have.length', count);
    }
  });
}

/**
 * Verify empty state is displayed
 */
export function verifyEmptyState(): void {
  cy.get(DEVICE_VIEW_SELECTORS.deviceList).within(() => {
    cy.get(DEVICE_VIEW_SELECTORS.emptyStateMessage).should('be.visible');
    cy.get(DEVICE_CARD_SELECTORS.card).should('not.exist');
  });
}

/**
 * Verify loading state is displayed via bootstrap busy dialog
 */
export function verifyLoadingState(): void {
  cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator).should('be.visible');
}

/**
 * Verify device state label displays expected state
 */
export function verifyDeviceState(options: {
  deviceIndex: number;
  state: string;
}): void {
  const { deviceIndex, state } = options;

  getDeviceCard(deviceIndex)
    .find(DEVICE_CARD_SELECTORS.stateLabel)
    .should('contain.text', state);
}

/**
 * Verify storage status for a device
 */
export function verifyStorageStatus(options: {
  deviceIndex: number;
  storageType: 'usb' | 'sd';
  available: boolean;
}): void {
  const { deviceIndex, storageType, available } = options;
  const selector =
    storageType === 'usb'
      ? DEVICE_CARD_SELECTORS.usbStorageStatus
      : DEVICE_CARD_SELECTORS.sdStorageStatus;

  getDeviceCard(deviceIndex)
    .find(selector)
    .should('be.visible')
    .within(() => {
      if (available) {
        cy.get(`[${DOM_ATTRIBUTES.DATA_TESTID}]`).should('not.have.class', CSS_CLASSES.UNAVAILABLE);
      } else {
        cy.get(`[${DOM_ATTRIBUTES.DATA_TESTID}]`).should('have.class', CSS_CLASSES.UNAVAILABLE);
      }
    });
}

/**
 * Verify all device information fields are displayed
 */
export function verifyFullDeviceInfo(deviceIndex: number): void {
  getDeviceCard(deviceIndex)
    .find(DEVICE_CARD_SELECTORS.deviceInfo)
    .within(() => {
      cy.get(DEVICE_CARD_SELECTORS.idLabel).should('be.visible');
      cy.get(DEVICE_CARD_SELECTORS.firmwareLabel).should('be.visible');
      cy.get(DEVICE_CARD_SELECTORS.portLabel).should('be.visible');
      cy.get(DEVICE_CARD_SELECTORS.stateLabel).should('be.visible');
      cy.get(DEVICE_CARD_SELECTORS.compatibleLabel).should('be.visible');
    });
}

/**
 * Click connect button for a device
 */
export function clickConnectDevice(deviceIndex: number): void {
  getDeviceCard(deviceIndex).find(DEVICE_CARD_SELECTORS.powerButton).click();
}

/**
 * Click disconnect button for a device
 */
export function clickDisconnectDevice(deviceIndex: number): void {
  getDeviceCard(deviceIndex).find(DEVICE_CARD_SELECTORS.powerButton).click();
}

/**
 * Verify error message is displayed
 */
export function verifyErrorMessage(): void {
  // Look for error in page text or common error selectors
  cy.get(DOM_ATTRIBUTES.BODY_TAG).should(($body) => {
    const hasErrorText = $body.text().toLowerCase().includes(CONSTANTS.ERROR_TEXT);
    const hasErrorElement =
      $body.find(`[role="${DOM_ATTRIBUTES.ROLE_ALERT}"]`).length > 0 ||
      $body.find(`.${CSS_CLASSES.ERROR_MESSAGE}`).length > 0 ||
      $body.find(`[${DOM_ATTRIBUTES.DATA_TESTID}*="${CONSTANTS.ERROR_TEXT}"]`).length > 0;

    // eslint-disable-next-line @typescript-eslint/no-unused-expressions
    expect(hasErrorText || hasErrorElement).to.be.true;
  });
}

/**
 * Click the Refresh Devices button to trigger device discovery
 */
export function clickRefreshDevices(): void {
  cy.contains('Refresh Devices').click();
}
