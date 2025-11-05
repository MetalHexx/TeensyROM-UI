/// <reference types="cypress" />

export {
  BUSY_DIALOG_SELECTORS,
  DEVICE_CARD_SELECTORS,
  DEVICE_TOOLBAR_SELECTORS,
  DEVICE_VIEW_SELECTORS,
  BUTTON_SELECTORS,
  ICON_CLASSES,
  CSS_CLASSES,
  DOM_ATTRIBUTES,
  CONSTANTS,
} from '../../support/constants/selector.constants';

import {
  DEVICE_CARD_SELECTORS,
  DEVICE_VIEW_SELECTORS,
  CONSTANTS,
  DOM_ATTRIBUTES,
  CSS_CLASSES,
} from '../../support/constants/selector.constants';
import { APP_ROUTES } from '../../support/constants/app-routes.constants';

/**
 * Navigate to the device view page with cleared storage
 */
export function navigateToDeviceView(): Cypress.Chainable<Cypress.AUTWindow> {
  return cy.visit(APP_ROUTES.devices, {
    onBeforeLoad: (win) => {
      win.localStorage.clear();
      win.sessionStorage.clear();
    },
  });
}

/**
 * Get device card element by index
 */
export function getDeviceCard(index: number): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get(DEVICE_CARD_SELECTORS.card).eq(index);
}

/**
 * Verify device card is visible at specified index
 */
export function expectDeviceCardVisible(index: number): void {
  getDeviceCard(index).should('be.visible');
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

  getDeviceCard(index)
    .should('be.visible')
    .within(() => {
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
export function verifyDeviceState(options: { deviceIndex: number; state: string }): void {
  const { deviceIndex, state } = options;

  getDeviceCard(deviceIndex).find(DEVICE_CARD_SELECTORS.stateLabel).should('contain.text', state);
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
 * Click connect button for device at specified index
 */
export function clickConnectDevice(deviceIndex: number): void {
  getDeviceCard(deviceIndex).find(DEVICE_CARD_SELECTORS.powerButton).click();
}

/**
 * Click disconnect button for device at specified index
 */
export function clickDisconnectDevice(deviceIndex: number): void {
  getDeviceCard(deviceIndex).find(DEVICE_CARD_SELECTORS.powerButton).click();
}

/**
 * Verify error message is displayed in the page
 */
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

/**
 * Click refresh devices button
 */
export function clickRefreshDevices(): void {
  cy.contains('Refresh Devices').click();
}

/**
 * Click power button for device at specified index
 */
export function clickPowerButton(deviceIndex: number): void {
  getDeviceCard(deviceIndex).find(DEVICE_CARD_SELECTORS.powerButton).click();
}

/**
 * Verify device is connected (not dimmed with highlighted power button)
 */
export function verifyConnected(deviceIndex: number): void {
  getDeviceCard(deviceIndex).should('not.have.class', CSS_CLASSES.DIMMED);
  getDeviceCard(deviceIndex)
    .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
    .should('have.class', 'highlight');
}

/**
 * Verify device is disconnected (dimmed with normal power button)
 */
export function verifyDisconnected(deviceIndex: number): void {
  getDeviceCard(deviceIndex).should('have.class', CSS_CLASSES.DIMMED);
  getDeviceCard(deviceIndex)
    .find(DEVICE_CARD_SELECTORS.powerButtonIcon)
    .should('have.class', 'normal');
}
