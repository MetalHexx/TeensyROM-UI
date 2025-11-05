import type MockFilesystem from '../../support/test-data/mock-filesystem/mock-filesystem';
import { createMockFilesystem } from '../../support/test-data/generators/storage.generators';
import { singleDevice } from '../../support/test-data/fixtures';
import { interceptConnectDevice } from '../../support/interceptors/connectDevice.interceptors';
import { interceptFindDevices } from '../../support/interceptors/findDevices.interceptors';
import { interceptGetDirectory } from '../../support/interceptors/getDirectory.interceptors';
import { interceptSaveFavorite, waitForSaveFavorite } from '../../support/interceptors/saveFavorite.interceptors';
import { interceptRemoveFavorite, waitForRemoveFavorite } from '../../support/interceptors/removeFavorite.interceptors';
import { interceptLaunchFile } from '../../support/interceptors/launchFile.interceptors';
import { VIEWPORT, MOCK_SEEDS, TIMEOUTS } from '../../support/constants/test.constants';
import { TeensyStorageType, TEST_FILES, TEST_PATHS } from '../../support/constants/storage.constants';
import { DIRECTORY_FILES_SELECTORS } from '../../support/constants/selector.constants';
import {
  navigateToPlayer,
  verifyFavoriteIconIsEmpty,
  clickFavoriteButton,
  clickFavoriteButtonAndWait,
  verifyFavoriteIconIsFilled,
  waitForPlayerToolbarVisible,
  waitForFavoriteButtonReady,
  clickFavoriteButtonAndWaitForRemove,
  launchFileFromFavorites,
  verifyFileNotInDirectory,
  setupSaveFavoriteErrorScenario,
  setupRemoveFavoriteErrorScenario,
  verifyErrorAlertDisplayed,
  verifyFavoriteStateUnchangedAfterError,
  verifyFavoriteButtonEnabledAfterError,
  navigateToDirectory,
  waitForDirectoryLoad,
} from './test-helpers';

describe('Favorites Functionality', () => {
  let filesystem: MockFilesystem;
  let testDeviceId: string;

  beforeEach(() => {
    cy.viewport(VIEWPORT.STANDARD.width, VIEWPORT.STANDARD.height);
    filesystem = createMockFilesystem(MOCK_SEEDS.DEFAULT);
    testDeviceId = singleDevice.devices[0].deviceId;
    interceptFindDevices({ fixture: singleDevice });
    interceptConnectDevice();
  });

  describe('Navigate to File and Verify Initial Favorite State', () => {
    it('should navigate to PAC_MAN via deep link and show empty favorite icon', () => {
      interceptGetDirectory({ filesystem });
      interceptLaunchFile({ filesystem });

      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      verifyFavoriteIconIsEmpty();
    });
  });

  describe('Favorite a File and Verify UI Updates', () => {
    it('should favorite PAC_MAN and show filled heart icon', () => {
      interceptGetDirectory({ filesystem });
      interceptLaunchFile({ filesystem });
      interceptSaveFavorite({ filesystem });

      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      waitForPlayerToolbarVisible();
      waitForFavoriteButtonReady();

      cy.log('About to click favorite button');
      clickFavoriteButtonAndWait();
      cy.log('Favorite button clicked and API call completed');

      verifyFavoriteIconIsFilled();
    });
  });

  describe('Unfavorite a File and Verify UI Updates', () => {
    it('should unfavorite PAC_MAN and show empty heart icon', () => {
      filesystem.saveFavorite(TEST_FILES.GAMES.PAC_MAN.filePath);

      interceptGetDirectory({ filesystem });
      interceptLaunchFile({ filesystem });
      interceptSaveFavorite({ filesystem });
      interceptRemoveFavorite({ filesystem });

      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      waitForPlayerToolbarVisible();
      waitForFavoriteButtonReady();

      cy.log('About to click favorite button to unfavorite');
      clickFavoriteButtonAndWaitForRemove();
      cy.log('Favorite button clicked and remove API call completed');

      verifyFavoriteIconIsEmpty();
    });
  });

  describe('File Disappears from Favorites Directory After Unfavoriting', () => {
    it('should remove PAC_MAN from favorites directory when unfavorited from player toolbar', () => {
      filesystem.saveFavorite(TEST_FILES.GAMES.PAC_MAN.filePath);

      interceptGetDirectory({ filesystem });
      interceptLaunchFile({ filesystem });
      interceptRemoveFavorite({ filesystem });

      launchFileFromFavorites({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        fileName: TEST_FILES.GAMES.PAC_MAN.fileName,
      });
      waitForPlayerToolbarVisible();
      waitForFavoriteButtonReady();
      verifyFavoriteIconIsFilled();

      clickFavoriteButtonAndWaitForRemove();

      // Force a directory refresh to ensure the UI reflects the favorite removal
      // This handles the timing gap between API completion and UI update
      cy.log('Forcing directory refresh to ensure UI is updated');
      navigateToDirectory({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.FAVORITES_GAMES,
      });
      waitForDirectoryLoad();

      // Wait for the file list to update after the directory refresh
      cy.get(DIRECTORY_FILES_SELECTORS.directoryFilesContainer, { timeout: TIMEOUTS.DEFAULT }).should('exist');

      cy.log('Verifying PAC_MAN disappeared from favorites directory');
      const favoritesFilePath = `${TEST_PATHS.FAVORITES_GAMES}/${TEST_FILES.GAMES.PAC_MAN.fileName}`;
      verifyFileNotInDirectory(favoritesFilePath);
    });
  });

  describe('API Error Handling and User Feedback', () => {
    it('should display error alert when save favorite operation fails', () => {
      setupSaveFavoriteErrorScenario(filesystem);

      interceptGetDirectory({ filesystem });
      interceptLaunchFile({ filesystem });

      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      waitForPlayerToolbarVisible();
      waitForFavoriteButtonReady();

      cy.log('About to click favorite button expecting error');
      clickFavoriteButton();
      waitForSaveFavorite();

      verifyErrorAlertDisplayed('Failed to save favorite. Please try again.');

      verifyFavoriteStateUnchangedAfterError('favorite_border');

      verifyFavoriteButtonEnabledAfterError();
    });

    it('should display error alert when remove favorite operation fails', () => {
      filesystem.saveFavorite(TEST_FILES.GAMES.PAC_MAN.filePath);
      setupRemoveFavoriteErrorScenario(filesystem);

      interceptGetDirectory({ filesystem });
      interceptLaunchFile({ filesystem });

      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      waitForPlayerToolbarVisible();
      waitForFavoriteButtonReady();

      cy.log('About to click favorite button expecting remove error');
      clickFavoriteButton();
      waitForRemoveFavorite();

      verifyErrorAlertDisplayed('Failed to remove favorite. Please try again.');

      verifyFavoriteStateUnchangedAfterError('favorite');

      verifyFavoriteButtonEnabledAfterError();
    });
  });
});