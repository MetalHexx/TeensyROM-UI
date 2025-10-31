/// <reference types="cypress" />

import { APP_ROUTES } from '../../support/constants/app-routes.constants';
import { TeensyStorageType, TEST_PATHS } from '../../support/constants/storage.constants';
import {
  PLAYER_TOOLBAR_SELECTORS,
  DIRECTORY_FILES_SELECTORS
} from '../../support/constants/selector.constants';
import { interceptSaveFavorite } from '../../support/interceptors/saveFavorite.interceptors';
import { interceptRemoveFavorite } from '../../support/interceptors/removeFavorite.interceptors';
import { verifyAlertVisible, verifyAlertMessage, verifyAlertIcon, verifyAlertSeverity } from '../../support/helpers/alert.helpers';
import type MockFilesystem from '../../support/test-data/mock-filesystem/mock-filesystem';

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
 * @param options Optional configuration including timeout and logging
 */
export function expectUrlContainsParams(params: {
  device?: string;
  storage?: string;
  path?: string;
  file?: string;
}, options?: {
  timeout?: number;
  logWaiting?: boolean;
}): void {
  const timeout = options?.timeout || 4000; // Cypress default timeout
  const logWaiting = options?.logWaiting || false;

  if (logWaiting) {
    cy.log(`Waiting for URL to update with file: ${params.file}`);
  }

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
  }).then((search) => {
    if (logWaiting) {
      cy.log(`✅ URL updated successfully: ${search}`);
    }
  });
}


/**
 * Click a file in the directory listing by path
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
  cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).dblclick();
}

/**
 * Click the next file button
 */
export function clickNextButton(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.nextButton).first().click();
}

/**
 * Click the previous file button
 */
export function clickPreviousButton(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.previousButton).first().click();
}

/**
 * Click the random file launch button
 */
export function clickRandomButton(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.randomButton).click();
}

/**
 * Assert a specific file is currently launched/playing
 */
export function expectFileIsLaunched(fileName: string): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo)
    .should('be.visible')
    .should('contain.text', fileName);
}

/**
 * Assert any file is currently launched (don't care which one)
 */
export function expectFileIsLaunchedAny(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo).should('be.visible');
}

/**
 * Assert no file is currently launched
 */
export function expectNoFileLaunched(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo).should('not.exist');
}

/**
 * Assert player toolbar is visible
 */
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
 * Wait for directory to load (GET_DIRECTORY API call completes)
 */
export function waitForDirectoryLoad(): void {
  cy.wait('@getDirectory');
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
 * Wait for file metadata to load after launch (GET_DIRECTORY API call completes)
 */
export function waitForFileToLoad(): void {
  cy.wait('@getDirectory');
}

/**
 * Wait for file launch API call to complete (LAUNCH_FILE interceptor)
 */
export function waitForFileLaunch(timeout = 10000): void {
  cy.wait('@launchFile', { timeout });
}

/**
 * Wait for random file launch API call to complete (LAUNCH_RANDOM interceptor)
 */
export function waitForRandomLaunch(): void {
  cy.wait('@launchRandom');
}

/**
 * Wait for file info to appear after launching a file
 */
export function waitForFileInfoToAppear(): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo, { timeout: 10000 }).first().should('be.visible');
}

/**
 * Go back in browser history
 */
export function goBack(): void {
  cy.go('back');
}

/**
 * Go forward in browser history
 */
export function goForward(): void {
  cy.go('forward');
}

// ============================================================================
// Favorites Testing Helpers
// ============================================================================


/**
 * Wait for device discovery to complete (FIND_DEVICES API call)
 */
export function waitForDeviceDiscovery(): void {
  cy.wait('@findDevices');
}

/**
 * Wait for device connection to complete (CONNECT_DEVICE API call)
 */
export function waitForDeviceConnection(): void {
  cy.wait('@connectDevice');
}

/**
 * Wait for save favorite API call to complete (SAVE_FAVORITE API call)
 */
export function waitForSaveFavorite(timeout = 10000): void {
  cy.wait('@saveFavorite', { timeout });
}

/**
 * Wait for remove favorite API call to complete (REMOVE_FAVORITE API call)
 */
export function waitForRemoveFavorite(timeout = 10000): void {
  cy.wait('@removeFavorite', { timeout });
}

/**
 * Wait for player toolbar to become visible
 */
export function waitForPlayerToolbarVisible(): void {
  cy.get(`${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible`, { timeout: 10000 }).should('be.visible');
}

/**
 * Wait for favorite button to be visible and enabled
 */
export function waitForFavoriteButtonReady(): void {
  cy.get(`${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteButton}`, { timeout: 10000 })
    .should('be.visible')
    .should('not.be.disabled');
}

/**
 * Wait for favorite icon to contain specific text content
 */
export function waitForFavoriteIconToContain(expectedIcon: string): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.favoriteIcon, { timeout: 10000 })
    .should('contain.text', expectedIcon);
}

// ============================================================================
// Favorites Action Helpers
// ============================================================================

/**
 * Click the favorite button in the player toolbar
 */
export function clickFavoriteButton(): void {
  cy.get(`${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteButton}`).click();
}

/**
 * Click favorite button and wait for save operation to complete
 */
export function clickFavoriteButtonAndWait(): void {
  clickFavoriteButton();
  waitForSaveFavorite();
}

/**
 * Click favorite button and wait for remove operation to complete
 */
export function clickFavoriteButtonAndWaitForRemove(): void {
  clickFavoriteButton();
  waitForRemoveFavorite();
}

// ============================================================================
// Favorites Verification Helpers
// ============================================================================

/**
 * Verify the favorite icon contains the expected text content
 */
export function verifyFavoriteIconState(expectedIcon: string): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.favoriteIcon).should('contain.text', expectedIcon);
}

/**
 * Verify the favorite icon shows empty state (favorite_border)
 */
export function verifyFavoriteIconIsEmpty(): void {
  cy.get(`${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteIcon}`).should('contain.text', 'favorite_border');
}

/**
 * Verify the favorite icon shows filled state (favorite)
 */
export function verifyFavoriteIconIsFilled(): void {
  cy.get(`${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteIcon}`).should('contain.text', 'favorite');
}

/**
 * Verify current file info shows expected file name
 */
export function verifyCurrentFileInfo(fileName: string): void {
  cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo)
    .should('be.visible')
    .should('contain.text', fileName);
}

// ============================================================================
// Favorites Directory Helpers
// ============================================================================

/**
 * Navigate to a specific directory path (favorites or regular)
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
 * Launch a specific file from favorites directory
 */
export function launchFileFromFavorites(params: {
  device?: string;
  storage?: TeensyStorageType;
  fileName: string;
}): void {
  navigateToDirectory({
    device: params.device,
    storage: TeensyStorageType.Sd,
    path: TEST_PATHS.FAVORITES_GAMES,
    file: params.fileName,
  });
  waitForDirectoryLoad();
  waitForFileLaunch();
  waitForDirectoryFilesToBeVisible(TEST_PATHS.FAVORITES_GAMES);

  const favoritesFilePath = `${TEST_PATHS.FAVORITES_GAMES}/${params.fileName}`;
  cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(favoritesFilePath))
    .should('be.visible')
    .click();
}

/**
 * Verify file does not exist in directory listing
 */
export function verifyFileNotInDirectory(filePath: string): void {
  cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).should('not.exist');
}

// ============================================================================
// Error Testing Helpers
// ============================================================================

/**
 * Set up error scenario for save favorite operations
 */
export function setupSaveFavoriteErrorScenario(filesystem?: MockFilesystem): void {
  interceptSaveFavorite({
    filesystem,
    errorMode: true,
    responseDelayMs: 500
  });
}

/**
 * Set up error scenario for remove favorite operations
 */
export function setupRemoveFavoriteErrorScenario(filesystem?: MockFilesystem): void {
  interceptRemoveFavorite({
    filesystem,
    errorMode: true,
    responseDelayMs: 500
  });
}

/**
 * Verify error alert is displayed with expected message
 */
export function verifyErrorAlertDisplayed(expectedMessage: string): void {
  verifyAlertVisible();
  verifyAlertMessage(expectedMessage);
  verifyAlertIcon('error');
  verifyAlertSeverity('ERROR');
}

/**
 * Verify favorite icon state remains unchanged after error
 */
export function verifyFavoriteStateUnchangedAfterError(expectedIcon: string): void {
  cy.get(`${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteIcon}`)
    .should('contain.text', expectedIcon);
}

/**
 * Verify favorite button is enabled after error scenario
 */
export function verifyFavoriteButtonEnabledAfterError(): void {
  cy.get(`${PLAYER_TOOLBAR_SELECTORS.toolbar}:visible ${PLAYER_TOOLBAR_SELECTORS.favoriteButton}`)
    .should('not.be.disabled');
}
