/* eslint-disable @typescript-eslint/no-explicit-any */
/**
 * Indexing E2E Test Helpers
 *
 * Reusable Cypress commands and helpers for indexing test suites.
 * These helpers compose Cypress commands, interceptor setup, and test fixtures.
 *
 * Imported dependencies:
 * - Interceptors: `storage-indexing.interceptors.ts` (interceptIndexStorage, interceptIndexAllStorage)
 * - Selectors: `selector.constants.ts` (STORAGE_INDEX_BUTTON_SELECTORS, BUSY_DIALOG_GENERIC_SELECTORS)
 * - Timeouts: `api.constants.ts` (TIMEOUTS)
 * - Storage Type: `@teensyrom-nx/data-access/api-client` (TeensyStorageType)
 */

import { STORAGE_INDEX_BUTTON_SELECTORS, BUSY_DIALOG_GENERIC_SELECTORS } from '../constants/selector.constants';
import { TIMEOUTS } from '../constants/api.constants';
import { TeensyStorageType } from '@teensyrom-nx/data-access/api-client';
import {
  interceptIndexStorage,
  interceptIndexAllStorage,
  INDEXING_INTERCEPT_ALIASES,
} from '../interceptors/storage-indexing.interceptors';

/**
 * Setup complete indexing scenario with devices and interceptors
 *
 * Prepares indexing interceptors for storage operations.
 * Call this to setup interceptors before navigating or during test setup.
 *
 * @param fixture Device fixture data - should have shape { devices: [...] }
 * @param interceptorOptions Optional configuration passed to all interceptors
 * @returns Cypress chainable for command chaining
 *
 * @example
 * // Setup with single device
 * setupIndexingScenario(deviceWithAvailableStorage(), { delay: 500 });
 *
 * // Setup with multiple devices and custom options
 * setupIndexingScenario(multipleDevicesForIndexing(), { delay: 1000 });
 */
export function setupIndexingScenario(
  fixture: { devices: readonly any[] },
  interceptorOptions?: { delay?: number; statusCode?: number; errorMode?: boolean; errorMessage?: string }
) {
  // Setup indexing interceptors for each device's storage
  fixture.devices.forEach((device: any) => {
    const deviceId = device.deviceId as string;
    const sdStorage = device.sdStorage as any;
    const usbStorage = device.usbStorage as any;

    if (sdStorage?.available) {
      interceptIndexStorage(deviceId, TeensyStorageType.Sd, interceptorOptions);
    }

    if (usbStorage?.available) {
      interceptIndexStorage(deviceId, TeensyStorageType.Usb, interceptorOptions);
    }
  });

  // Setup Index All interceptor
  interceptIndexAllStorage(interceptorOptions);
}

/**
 * Verify busy dialog is displayed
 *
 * Waits for indexing dialog to appear by checking for its specific title "Indexing Storage".
 * The indexing dialog is identified by its title, not message content.
 *
 * @param timeout Optional timeout for dialog appearance (default: 1000ms)
 * @returns Cypress chainable for command chaining
 *
 * @example
 * verifyBusyDialogDisplayed();
 * verifyBusyDialogDisplayed(2000); // Wait up to 2s for dialog to appear
 */
export function verifyBusyDialogDisplayed(
  timeout: number = TIMEOUTS.DIALOG_APPEARANCE
): Cypress.Chainable<JQuery<HTMLElement>> {
  // Wait for the indexing dialog by checking for its specific title text
  cy.get('h2', { timeout })
    .contains('Indexing Storage')
    .should('exist');

  return cy.get(BUSY_DIALOG_GENERIC_SELECTORS.container, { timeout });
}

/**
 * Verify busy dialog is hidden
 *
 * Asserts that the indexing dialog (identified by "Indexing Storage" title) is no longer displayed.
 *
 * @param timeout Optional timeout for dialog to disappear (default: 2000ms)
 * @returns Cypress chainable for command chaining
 *
 * @example
 * verifyBusyDialogHidden();
 * verifyBusyDialogHidden(5000); // Wait up to 5s for dialog to hide
 */
export function verifyBusyDialogHidden(timeout = 2000): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy
    .get('h2', { timeout })
    .contains('Indexing Storage')
    .should('not.exist');
}

/**
 * Verify storage index button enable/disable state
 *
 * Checks if USB or SD storage index button is enabled or disabled.
 * Useful for verifying button state during and after indexing operations.
 *
 * @param storageType Storage type: 'usb' or 'sd'
 * @param shouldBeDisabled If true, button should be disabled; if false, should be enabled
 * @returns Cypress chainable for command chaining
 *
 * @example
 * verifyStorageIndexButtonState('usb', false); // USB button should be enabled
 * verifyStorageIndexButtonState('sd', true);   // SD button should be disabled during indexing
 */
export function verifyStorageIndexButtonState(
  storageType: 'usb' | 'sd',
  shouldBeDisabled: boolean
): Cypress.Chainable<JQuery<HTMLElement>> {
  const selector = STORAGE_INDEX_BUTTON_SELECTORS.byType(storageType);
  const expectation = shouldBeDisabled ? 'be.disabled' : 'not.be.disabled';

  return cy.get(selector).should(expectation);
}

/**
 * Click storage index button to trigger indexing
 *
 * Clicks the USB or SD storage index button.
 * Typically followed by waitForIndexingComplete() to track the API call.
 *
 * @param storageType Storage type: 'usb' or 'sd'
 * @returns Cypress chainable for command chaining
 *
 * @example
 * clickStorageIndexButton('usb');
 * clickStorageIndexButton('sd');
 */
export function clickStorageIndexButton(storageType: 'usb' | 'sd'): Cypress.Chainable<JQuery<HTMLElement>> {
  const selector = STORAGE_INDEX_BUTTON_SELECTORS.byType(storageType);
  return cy.get(selector).click();
}

/**
 * Click Index All button to trigger batch indexing
 *
 * Clicks the toolbar's "Index All" button to start indexing all connected devices.
 * Typically followed by waitForIndexingComplete() to track the API call.
 *
 * @returns Cypress chainable for command chaining
 *
 * @example
 * clickIndexAllButton();
 */
export function clickIndexAllButton(): Cypress.Chainable<JQuery<HTMLElement>> {
  return cy.get('[data-testid="toolbar-button-index-all"] button').click();
}

/**
 * Wait for indexing API call to complete
 *
 * Waits for a specific indexing operation API call using Cypress cy.wait().
 * Validates the interceptor alias to ensure the call was made and completes.
 *
 * @param deviceIdOrAlias Device ID for single-device indexing, or INDEXING_INTERCEPT_ALIASES constant for Index All
 * @param storageType Optional storage type for single-device operations ('USB' or 'SD')
 * @param timeout Optional timeout for API response (default: 10000ms)
 * @returns Cypress chainable with intercept response
 *
 * @example
 * // Wait for single device USB indexing
 * waitForIndexingComplete('device-id-123', 'USB');
 *
 * // Wait for Index All operation
 * waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
 *
 * // Wait with custom timeout
 * waitForIndexingComplete('device-id-123', 'SD', 15000);
 */
export function waitForIndexingComplete(
  deviceIdOrAlias: string,
  storageType?: 'USB' | 'SD',
  timeout = TIMEOUTS.API_RESPONSE
): Cypress.Chainable<any> {
  // Determine alias based on parameters
  let alias: string;

  if (storageType) {
    // Single device operation
    alias = INDEXING_INTERCEPT_ALIASES.byDeviceAndType(deviceIdOrAlias, storageType);
  } else {
    // Assume it's already an alias (e.g., INDEX_ALL_STORAGE)
    alias = deviceIdOrAlias;
  }

  return cy.wait(`@${alias}`, { timeout });
}

/**
 * Verify indexing API call was made
 *
 * Asserts that a specific indexing operation API call was made.
 * Useful for verification after async operations complete.
 *
 * @param deviceId Device ID for the indexing operation
 * @param storageType Storage type ('USB' or 'SD')
 * @returns Cypress chainable for command chaining
 *
 * @example
 * clickStorageIndexButton('usb');
 * waitForIndexingComplete('device-id-123', 'USB');
 * verifyIndexingCallMade('device-id-123', 'USB'); // Optional verification
 */
export function verifyIndexingCallMade(deviceId: string, storageType: 'USB' | 'SD'): Cypress.Chainable<any> {
  const alias = INDEXING_INTERCEPT_ALIASES.byDeviceAndType(deviceId, storageType);
  return cy.get(`@${alias}`).should('exist') as any;
}

/**
 * Verify Index All call was made
 *
 * Asserts that the Index All batch operation API call was made.
 *
 * @returns Cypress chainable for command chaining
 *
 * @example
 * clickIndexAllButton();
 * waitForIndexingComplete(INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE);
 * verifyIndexAllCallMade();
 */
export function verifyIndexAllCallMade(): Cypress.Chainable<any> {
  return cy.get(`@${INDEXING_INTERCEPT_ALIASES.INDEX_ALL_STORAGE}`).should('exist') as any;
}
