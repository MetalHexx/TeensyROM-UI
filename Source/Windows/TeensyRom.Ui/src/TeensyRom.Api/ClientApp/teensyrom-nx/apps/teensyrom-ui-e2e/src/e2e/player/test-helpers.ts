/// <reference types="cypress" />

import { APP_ROUTES } from '../../support/constants/app-routes.constants';
import { INTERCEPT_ALIASES } from '../../support/constants/api.constants';
import {
  PLAYER_TOOLBAR_SELECTORS,
  DIRECTORY_FILES_SELECTORS,
} from '../../support/constants/selector.constants';

export * from '../../support/helpers/alert.helpers';
export { INTERCEPT_ALIASES as API_ROUTE_ALIASES } from '../../support/constants/api.constants';

/**
 * Navigate to the player with optional query parameters
 */
export function navigateToPlayerWithParams(params?: {
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

  return cy.visit(url, {
    onBeforeLoad: (win) => {
      win.localStorage.clear();
      win.sessionStorage.clear();
    },
  });
}

/**
 * Navigate to player view without parameters
 */
export function navigateToPlayerView(): Cypress.Chainable<Cypress.AUTWindow> {
  return navigateToPlayerWithParams();
}

/**
 * Assert URL contains expected parameters
 * Note: Handles both encoded and non-encoded paths
 * @param params Query parameters to verify in URL
 * @param timeout Optional timeout to wait for URL to update (e.g., after async file launches). Default waits with Cypress default timeout.
 */
export function expectUrlContainsParams(params: {
  device?: string;
  storage?: string;
  path?: string;
  file?: string;
}, timeout?: number): void {
  const options = timeout ? { timeout } : {};
  cy.location('search', options).should((search) => {
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
  // Scroll into view first to handle virtual scrolling
  cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).scrollIntoView();
  // Then double-click in a separate chain
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
    // Scroll into view to handle virtual scrolling clipping
    cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).scrollIntoView();
    // Then verify visibility in a separate chain
    cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).should('be.visible');
  } else {
    cy.get(DIRECTORY_FILES_SELECTORS.fileListItem(filePath)).should('not.exist');
  }
}

/**
 * Wait for directory to load (GET_DIRECTORY API call completes)
 */
export function waitForDirectoryLoad(): void {
  cy.wait(`@${INTERCEPT_ALIASES.GET_DIRECTORY}`);
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
  cy.wait(`@${INTERCEPT_ALIASES.GET_DIRECTORY}`);
}

/**
 * Wait for file launch API call to complete (LAUNCH_FILE interceptor)
 */
export function waitForFileLaunch(): void {
  cy.wait(`@${INTERCEPT_ALIASES.LAUNCH_FILE}`);
}

/**
 * Wait for random file launch API call to complete (LAUNCH_RANDOM interceptor)
 */
export function waitForRandomLaunch(): void {
  cy.wait(`@${INTERCEPT_ALIASES.LAUNCH_RANDOM}`);
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
