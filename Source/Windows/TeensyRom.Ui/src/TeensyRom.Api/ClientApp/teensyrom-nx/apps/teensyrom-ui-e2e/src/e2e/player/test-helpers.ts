/// <reference types="cypress" />

/**
 * Player Test Helpers - DOM/UI Interaction Utilities
 *
 * This file provides comprehensive test helper functions for player E2E tests.
 * It contains exclusively DOM interaction and UI verification utilities.
 *
 * NOTE: API-related functions (waiters, interceptors, error setup), look at the interceptors.
 */

import { APP_ROUTES } from '../../support/constants/app-routes.constants';
import { TeensyStorageType } from '../../support/constants/storage.constants';
import {
  PLAYER_TOOLBAR_SELECTORS,
  DIRECTORY_FILES_SELECTORS,
} from '../../support/constants/selector.constants';
import {
  verifyAlertVisible,
  verifyAlertMessage,
  verifyAlertIcon,
  verifyAlertSeverity,
} from '../../support/helpers/alert.helpers';

export * from '../../support/helpers/alert.helpers';

/**
 * Navigate to the player with optional query parameters
 * @param params Optional query parameters for device, storage, path, and file
 */
export function navigateToPlayer(params?: {
  device?: string;
  storage?: string;
  path?: string;
  file?: string;
}): Cypress.Chainable<Cypress.AUTWindow> {
  const queryParams = new URLSearchParams();
  if (params?.device) queryParams.append('device', params.device);
  if (params?.storage) queryParams.append('storage', params.storage);
  if (params?.path) queryParams.append('path', params.path);
  if (params?.file) queryParams.append('file', params.file);

  const url = `${APP_ROUTES.player}${queryParams.toString() ? '?' + queryParams.toString() : ''}`;

  return cy.visit(url);
}

/**
 * Assert URL contains expected parameters
 * @param params Query parameters to verify in URL
 * @param options Optional configuration including timeout
 */
export function expectUrlContainsParams(
  params: {
    device?: string;
    storage?: string;
    path?: string;
    file?: string;
  },
  options?: { timeout?: number }
): void {
  const timeout = options?.timeout || 10000;

  cy.location('search', { timeout }).should((search) => {
    if (params.device) expect(search).to.include(`device=${params.device}`);
    if (params.storage) expect(search).to.include(`storage=${params.storage}`);
    // Path can be either encoded (%2F) or not encoded (/)
    if (params.path) {
      const encoded = `path=${encodeURIComponent(params.path)}`;
      const notEncoded = `path=${params.path}`;
      expect(search).to.satisfy((s: string) => s.includes(encoded) || s.includes(notEncoded));
    }
    if (params.file) expect(search).to.include(`file=${encodeURIComponent(params.file)}`);
  });
}

/**
 * Click a file in the directory listing
 * @param filePath Full path to the file
 */
export function clickFileInDirectory(filePath: string): void {
  cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).click();
}

/**
 * Double-click a file in the directory listing to launch it
 * @param filePath Full path to the file (e.g., "/games/Pac-Man (J1).crt")
 */
export function doubleClickFileInDirectory(filePath: string): void {
  cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).scrollIntoView();
  cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).dblclick({ force: true });
}

/** Click next button in player toolbar */
export function clickNextButton(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.nextButton).first().click();
}

/** Click previous button in player toolbar */
export function clickPreviousButton(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.previousButton).first().click();
}

/** Click random button in player toolbar */
export function clickRandomButton(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.randomButton).click();
}

/**
 * Verify a specific file is currently launched/playing
 * @param fileName The file name to verify
 */
export function expectFileIsLaunched(fileName: string): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo).should('exist').should('contain.text', fileName);
}

/** Verify any file is currently launched/playing */
export function expectFileIsLaunchedAny(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo).should('exist');
}

/** Verify no file is currently launched/playing */
export function expectNoFileLaunched(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo).should('not.exist');
}

/** Verify player toolbar is visible */
export function expectPlayerToolbarVisible(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.toolbar).should('be.visible');
}

/**
 * Verify a file appears in the directory listing
 * @param filePath Full path to the file (e.g., "/games/Pac-Man (J1).crt")
 * @param shouldBeVisible Whether the file should be visible (default: true)
 */
export function verifyFileInDirectory(filePath: string, shouldBeVisible = true): void {
  if (shouldBeVisible) {
    cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).scrollIntoView();
    cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).should('be.visible');
  } else {
    cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).should('not.exist');
  }
}

/**
 * Wait for directory files to be visible in the listing
 * @param pathPrefix Optional path prefix to match files (e.g., "/games/")
 */
export function waitForDirectoryFilesToBeVisible(pathPrefix = '/'): void {
  cy.get(DIRECTORY_FILES_SELECTORS.fileListItemsByPrefix(pathPrefix), { timeout: 10000 })
    .first()
    .should('be.visible');
}

/**
 * Wait for file info to appear in the player toolbar
 */
export function waitForFileInfoToAppear(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo, { timeout: 10000 }).first().should('exist');
}

/** Navigate back in browser history */
export function goBack(): void {
  cy.go('back');
}

/** Navigate forward in browser history */
export function goForward(): void {
  cy.go('forward');
}

/**
 * Wait for player toolbar to become visible (with timeout)
 */
export function waitForPlayerToolbarVisible(): void {
  cy.get(`${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible`, { timeout: 10000 }).should('be.visible');
}

/**
 * Wait for favorite button to be visible and enabled
 */
export function waitForFavoriteButtonReady(): void {
  cy.get(`${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteButton}`, {
    timeout: 10000,
  })
    .should('be.visible')
    .should('not.be.disabled');
}

/**
 * Wait for favorite icon to display expected state
 * @param expectedIcon The icon text to match (e.g., "favorite" or "favorite_border")
 */
export function waitForFavoriteIconToContain(expectedIcon: string): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.favoriteIcon, { timeout: 10000 }).should(
    'contain.text',
    expectedIcon
  );
}

/** Click favorite button in player toolbar */
export function clickFavoriteButton(): void {
  cy.get(
    `${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteButton}`
  ).click();
}

/**
 * Verify the favorite icon displays the expected state
 * @param expectedIcon The icon text to verify (e.g., "favorite" or "favorite_border")
 */
export function verifyFavoriteIconState(expectedIcon: string): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.favoriteIcon).should('contain.text', expectedIcon);
}

/** Verify favorite icon shows empty state */
export function verifyFavoriteIconIsEmpty(): void {
  cy.get(
    `${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteIcon}`
  ).should('contain.text', 'favorite_border');
}

/** Verify favorite icon shows filled state */
export function verifyFavoriteIconIsFilled(): void {
  cy.get(
    `${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteIcon}`
  ).should('contain.text', 'favorite');
}

/**
 * Verify the current file being played
 * @param fileName The file name to verify
 */
export function verifyCurrentFileInfo(fileName: string): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo).should('exist').should('contain.text', fileName);
}

/**
 * Navigate to a specific directory with optional file and device parameters
 * @param params Navigation parameters including path, device, storage, and optional file
 */
export function navigateToDirectory(params: {
  device?: string;
  storage?: TeensyStorageType;
  path: string;
  file?: string;
}): void {
  const queryParams = new URLSearchParams();
  if (params.device) queryParams.append('device', params.device);
  if (params.storage) queryParams.append('storage', params.storage);
  if (params.path) queryParams.append('path', params.path);
  if (params.file) queryParams.append('file', params.file);

  const url = `${APP_ROUTES.player}${queryParams.toString() ? '?' + queryParams.toString() : ''}`;

  cy.visit(url);
}

/**
 * Verify file doesn't exist in directory listing
 * @param filePath Full path to the file
 */
export function verifyFileNotInDirectory(filePath: string): void {
  cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).should('not.exist');
}

/**
 * Verify error alert is displayed with expected message, icon, and severity
 * @param expectedMessage The error message to verify
 */
export function verifyErrorAlertDisplayed(expectedMessage: string): void {
  verifyAlertVisible();
  verifyAlertMessage(expectedMessage);
  verifyAlertIcon('error');
  verifyAlertSeverity('ERROR');
}

/**
 * Verify favorite icon state remains unchanged after error
 * @param expectedIcon The icon state that should persist
 */
export function verifyFavoriteStateUnchangedAfterError(expectedIcon: string): void {
  cy.get(
    `${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteIcon}`
  ).should('contain.text', expectedIcon);
}

/** Verify favorite button remains enabled after error */
export function verifyFavoriteButtonEnabledAfterError(): void {
  cy.get(
    `${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteButton}`
  ).should('not.be.disabled');
}
