/// <reference types="cypress" />

/**
 * Device Discovery & Connection E2E Test Helpers & Selectors
 *
 * Centralized test utilities and data-testid selectors for device lifecycle tests.
 * This file contains reusable helper functions and a single source of truth for
 * all element selectors used across device discovery and connection workflow tests.
 *
 * **Sections**:
 * - Constants: Routes, aliases, CSS classes, DOM attributes
 * - Selectors: Centralized element selectors for device cards, storage, etc.
 * - Device Discovery Helpers: Navigate, wait for discovery, verify device lists
 * - Device Connection Helpers (Phase 1): Connect/disconnect, wait, verify state
 */

// Re-export centralized API constants for use in tests
export { INTERCEPT_ALIASES as API_ROUTE_ALIASES, DEVICE_ENDPOINTS, API_CONFIG, createProblemDetailsResponse } from '../../support/constants/api.constants';

// Re-export centralized UI selectors for use in tests
export {
  ALERT_SELECTORS,
  BUSY_DIALOG_SELECTORS,
  DEVICE_CARD_SELECTORS,
  DEVICE_VIEW_SELECTORS,
  BUTTON_SELECTORS,
  UI_SELECTORS,
  getAlertMessageSelector,
  getAlertIconSelector,
  getDeviceCardByIndexSelector,
  getByTestId,
  getByClass,
} from '../../support/constants/selector.constants';

// Import for local use in helper functions
import { DEVICE_CARD_SELECTORS, DEVICE_VIEW_SELECTORS } from '../../support/constants/selector.constants';
import { INTERCEPT_ALIASES } from '../../support/constants/api.constants';

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
  cy.wait(`@${INTERCEPT_ALIASES.FIND_DEVICES}`, { timeout });
  // Also wait for at least one device card to appear in the DOM
  cy.get(DEVICE_CARD_SELECTORS.card, { timeout: 5000 }).should('have.length.at.least', 1);
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

// ============================================================================
// CONNECTION WORKFLOW HELPERS (Phase 1)
// ============================================================================

/**
 * Click the power button on a device card to initiate connect/disconnect
 * 
 * The power button acts as a toggle:
 * - If device is disconnected, clicking connects it
 * - If device is connected, clicking disconnects it
 * 
 * @param deviceIndex - 0-based index of the device card
 * 
 * @example
 * ```typescript
 * clickPowerButton(0); // Click power button on first device
 * ```
 */
export function clickPowerButton(deviceIndex: number): void {
  getDeviceCard(deviceIndex).find(DEVICE_CARD_SELECTORS.powerButton).click();
}

/**
 * Wait for the connection API call to complete
 * 
 * Uses the @connectDevice alias from the interceptor.
 * Default timeout uses CONSTANTS.DEFAULT_TIMEOUT (5000ms).
 * 
 * @param timeout - Optional timeout in milliseconds (default: 5000)
 * 
 * @example
 * ```typescript
 * clickPowerButton(0);
 * waitForConnection();
 * verifyConnected(0);
 * ```
 */
export function waitForConnection(timeout = CONSTANTS.DEFAULT_TIMEOUT): void {
  cy.wait(`@${INTERCEPT_ALIASES.CONNECT_DEVICE}`, { timeout });
}

/**
 * Wait for the disconnection API call to complete
 * 
 * Uses the @disconnectDevice alias from the interceptor.
 * Default timeout uses CONSTANTS.DEFAULT_TIMEOUT (5000ms).
 * 
 * @param timeout - Optional timeout in milliseconds (default: 5000)
 * 
 * @example
 * ```typescript
 * clickPowerButton(0);
 * waitForDisconnection();
 * verifyDisconnected(0);
 * ```
 */
export function waitForDisconnection(timeout = CONSTANTS.DEFAULT_TIMEOUT): void {
  cy.wait(`@${INTERCEPT_ALIASES.DISCONNECT_DEVICE}`, { timeout });
}

/**
 * Verify a device is in connected state
 * 
 * Checks:
 * - Device card does NOT have 'dimmed' class (visual indicator of disconnected)
 * - Power button's mat-icon has 'highlight' class (visual indicator of connected)
 * 
 * @param deviceIndex - 0-based index of the device card
 * 
 * @example
 * ```typescript
 * verifyConnected(0); // Verify first device is connected
 * ```
 */
export function verifyConnected(deviceIndex: number): void {
  getDeviceCard(deviceIndex).should('not.have.class', CSS_CLASSES.DIMMED);
  
  // Verify power button icon has 'highlight' class
  getDeviceCard(deviceIndex)
    .find(DEVICE_CARD_SELECTORS.powerButton)
    .find('mat-icon')
    .should('have.class', 'highlight');
}

/**
 * Verify a device is in disconnected state
 * 
 * Checks:
 * - Device card HAS 'dimmed' class (visual indicator of disconnected)
 * - Power button's mat-icon has 'normal' class (visual indicator of disconnected)
 * 
 * @param deviceIndex - 0-based index of the device card
 * 
 * @example
 * ```typescript
 * verifyDisconnected(0); // Verify first device is disconnected
 * ```
 */
export function verifyDisconnected(deviceIndex: number): void {
  getDeviceCard(deviceIndex).should('have.class', CSS_CLASSES.DIMMED);
  
  // Verify power button icon has 'normal' class
  getDeviceCard(deviceIndex)
    .find(DEVICE_CARD_SELECTORS.powerButton)
    .find('mat-icon')
    .should('have.class', 'normal');
}
