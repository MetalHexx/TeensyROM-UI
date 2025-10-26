/// <reference types="cypress" />

import { APP_ROUTES } from '../../support/constants/app-routes.constants';
import { ALERT_SELECTORS } from '../../support/constants/selector.constants';
import { INTERCEPT_ALIASES } from '../../support/constants/api.constants';

const SELECTORS = {
  favoriteButton: 'button[aria-label*="Favorites"]',
  favoriteIcon: 'button[aria-label*="Favorites"] mat-icon',
  directoryTreeNode: 'mat-tree-node',
  fileListItem: '.file-list-item',
};

export const favoriteButtonSelector = SELECTORS.favoriteButton;

function escapeForRegex(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function directoryNodeContains(text: string): Cypress.Chainable<JQuery<HTMLElement>> {
  const pattern = new RegExp(`^\\s*${escapeForRegex(text)}\\s*$`, 'i');
  return cy.get(SELECTORS.directoryTreeNode).filter((_idx, el) => pattern.test(el.textContent ?? ''));
}

export function navigateToPlayerView(): Cypress.Chainable<Cypress.AUTWindow> {
  return cy.visit(APP_ROUTES.player, {
    onBeforeLoad: (win) => {
      win.localStorage.clear();
      win.sessionStorage.clear();
    },
  });
}

export function loadFileInPlayer(filePath: string): Cypress.Chainable<Cypress.AUTWindow>;
export function loadFileInPlayer(params: { deviceId?: string; storageType?: 'SD' | 'USB'; path?: string; filePath?: string }): Cypress.Chainable<Cypress.AUTWindow>;
export function loadFileInPlayer(
  filePathOrParams: string | { deviceId?: string; storageType?: 'SD' | 'USB'; path?: string; filePath?: string }
): Cypress.Chainable<Cypress.AUTWindow> {
  let url: string;

  if (typeof filePathOrParams === 'string') {
    // Legacy signature: loadFileInPlayer('/path/to/file')
    url = `${APP_ROUTES.player}?filePath=${encodeURIComponent(filePathOrParams)}`;
  } else {
    // New route parameter signature
    const params = filePathOrParams;
    const queryParams = new URLSearchParams();
    if (params.deviceId) queryParams.append('device', params.deviceId);
    if (params.storageType) queryParams.append('storage', params.storageType);
    if (params.path) queryParams.append('path', params.path);
    if (params.filePath) queryParams.append('filePath', params.filePath);
    url = `${APP_ROUTES.player}${queryParams.toString() ? '?' + queryParams.toString() : ''}`;
  }

  return cy.visit(url, {
    onBeforeLoad: (win) => {
      const deviceState = win.sessionStorage.getItem('connectedDevice');
      win.sessionStorage.clear();
      if (deviceState) {
        win.sessionStorage.setItem('connectedDevice', deviceState);
      }
    },
  });
}

export function buildPlayerUrl(params: { deviceId?: string; storageType?: 'SD' | 'USB'; path?: string; filePath?: string }): string {
  const queryParams = new URLSearchParams();
  if (params.deviceId) queryParams.append('device', params.deviceId);
  if (params.storageType) queryParams.append('storage', params.storageType);
  if (params.path) queryParams.append('path', params.path);
  if (params.filePath) queryParams.append('filePath', params.filePath);
  return `${APP_ROUTES.player}${queryParams.toString() ? '?' + queryParams.toString() : ''}`;
}

export function openFavoritesDirectory(type: 'games' | 'music' | 'images'): void {
  directoryNodeContains('favorites').first().click();
  directoryNodeContains(type).first().click();
}

export function clickFavoriteButton(): void {
  cy.get(SELECTORS.favoriteButton).click();
}

export function expectFavoriteIcon(expected: 'favorite' | 'favorite_border'): void {
  cy.get(SELECTORS.favoriteIcon)
    .invoke('text')
    .then((text) => text.trim())
    .should('equal', expected);
}

export function expectFavoriteButtonDisabled(expectDisabled: boolean): void {
  const assertion: 'be.disabled' | 'not.be.disabled' = expectDisabled ? 'be.disabled' : 'not.be.disabled';
  cy.get(SELECTORS.favoriteButton).should(assertion);
}

export function verifyAlert(message: string): void {
  cy.get(ALERT_SELECTORS.container)
    .should('be.visible')
    .within(() => {
      cy.get(ALERT_SELECTORS.message).should('contain.text', message);
    });
}

export function verifyFileInDirectory(fileName: string, shouldExist: boolean): void {
  cy.get(SELECTORS.fileListItem).then(($items) => {
    const hasMatch = $items.toArray().some((item) => item.textContent?.includes(fileName));
    expect(hasMatch).to.eq(shouldExist);
  });
}

export function waitForSaveFavorite(): void {
  cy.wait(`@${INTERCEPT_ALIASES.SAVE_FAVORITE}`);
}

export function waitForRemoveFavorite(): void {
  cy.wait(`@${INTERCEPT_ALIASES.REMOVE_FAVORITE}`);
}

export function waitForDirectoryLoad(): void {
  cy.wait(`@${INTERCEPT_ALIASES.GET_DIRECTORY}`);
}
