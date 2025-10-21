/// <reference types="cypress" />

export { INTERCEPT_ALIASES as API_ROUTE_ALIASES, DEVICE_ENDPOINTS, API_CONFIG, createProblemDetailsResponse } from '../../support/constants/api.constants';

export {
  ALERT_SELECTORS,
  BUSY_DIALOG_SELECTORS,
  DEVICE_CARD_SELECTORS,
  DEVICE_VIEW_SELECTORS,
  BUTTON_SELECTORS,
  ICON_CLASSES,
  CSS_CLASSES,
  DOM_ATTRIBUTES,
  CONSTANTS,
  UI_SELECTORS,
  getAlertMessageSelector,
  getAlertIconSelector,
  getDeviceCardByIndexSelector,
  getByTestId,
  getByClass,
} from '../../support/constants/selector.constants';

import { DEVICE_CARD_SELECTORS, DEVICE_VIEW_SELECTORS } from '../../support/constants/selector.constants';
import { CONSTANTS, DOM_ATTRIBUTES, CSS_CLASSES } from '../../support/constants/selector.constants';
import { INTERCEPT_ALIASES } from '../../support/constants/api.constants';
import { APP_ROUTES } from '../../support/constants/app-routes.constants';

// Helper functions

export function navigateToDeviceView(): Cypress.Chainable<Cypress.AUTWindow> {
  return cy.visit(APP_ROUTES.devices, {
    onBeforeLoad: (win) => {
      win.localStorage.clear();
      win.sessionStorage.clear();
    },
  });
}

export function waitForDeviceDiscovery(timeout = 10000): void {
  cy.wait(`@${INTERCEPT_ALIASES.FIND_DEVICES}`, { timeout });
}

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
    if (name) {
      cy.get(DEVICE_CARD_SELECTORS.idLabel).should('contain.text', name);
    }

    if (port) {
      cy.get(DEVICE_CARD_SELECTORS.portLabel).should('contain.text', port);
    }

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
export function verifyLoadingState(timeout = 4000): void {
  cy.get(DEVICE_VIEW_SELECTORS.loadingIndicator, { timeout }).should('be.visible');
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

export function clickConnectDevice(deviceIndex: number): void {
  getDeviceCard(deviceIndex).find(DEVICE_CARD_SELECTORS.powerButton).click();
}

export function clickDisconnectDevice(deviceIndex: number): void {
  getDeviceCard(deviceIndex).find(DEVICE_CARD_SELECTORS.powerButton).click();
}

export function verifyErrorMessage(): void {
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

export function clickRefreshDevices(): void {
  cy.contains('Refresh Devices').click();
}

// Connection workflow helpers

export function clickPowerButton(deviceIndex: number): void {
  getDeviceCard(deviceIndex).find(DEVICE_CARD_SELECTORS.powerButton).click();
}

export function waitForConnection(timeout = CONSTANTS.DEFAULT_TIMEOUT): void {
  cy.wait(`@${INTERCEPT_ALIASES.CONNECT_DEVICE}`, { timeout });
}

export function waitForDisconnection(timeout = CONSTANTS.DEFAULT_TIMEOUT): void {
  cy.wait(`@${INTERCEPT_ALIASES.DISCONNECT_DEVICE}`, { timeout });
}

export function verifyConnected(deviceIndex: number): void {
  getDeviceCard(deviceIndex).should('not.have.class', CSS_CLASSES.DIMMED);
  getDeviceCard(deviceIndex)
    .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
    .should('have.class', 'highlight');
}

export function verifyDisconnected(deviceIndex: number): void {
  getDeviceCard(deviceIndex).should('have.class', CSS_CLASSES.DIMMED);
  getDeviceCard(deviceIndex)
    .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
    .should('have.class', 'normal');
}
