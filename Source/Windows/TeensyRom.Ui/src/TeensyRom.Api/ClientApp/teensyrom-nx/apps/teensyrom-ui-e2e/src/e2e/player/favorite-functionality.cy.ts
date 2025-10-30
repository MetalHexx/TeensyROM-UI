import type MockFilesystem from '../../support/test-data/mock-filesystem/mock-filesystem';
import { createMockFilesystem } from '../../support/test-data/generators/storage.generators';
import { singleDevice } from '../../support/test-data/fixtures';
import { interceptConnectDevice } from '../../support/interceptors/connectDevice.interceptors';
import { interceptFindDevices } from '../../support/interceptors/findDevices.interceptors';
import { interceptGetDirectory } from '../../support/interceptors/getDirectory.interceptors';
import { interceptSaveFavorite } from '../../support/interceptors/saveFavorite.interceptors';
import { interceptRemoveFavorite } from '../../support/interceptors/removeFavorite.interceptors';
import { interceptLaunchFile } from '../../support/interceptors/launchFile.interceptors';
import { VIEWPORT, MOCK_SEEDS } from '../../support/constants/test.constants';
import { TeensyStorageType, TEST_FILES, TEST_PATHS } from '../../support/constants/storage.constants';
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
} from './test-helpers';

describe('Favorites Functionality', () => {
  let filesystem: MockFilesystem;
  let testDeviceId: string;

  beforeEach(() => {

    cy.viewport(VIEWPORT.STANDARD.width, VIEWPORT.STANDARD.height);
    // Create deterministic mock filesystem with default seed
    filesystem = createMockFilesystem(MOCK_SEEDS.DEFAULT);

    // Get the device ID from the fixture (it's a generated UUID)
    testDeviceId = singleDevice.devices[0].deviceId;

    // Setup API interceptors for common requests
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

      // GIVEN: User navigates to PAC_MAN file via deep link
      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      // Wait for UI to be fully loaded
      waitForPlayerToolbarVisible();
      waitForFavoriteButtonReady();

      // WHEN: User clicks favorite button
      cy.log('About to click favorite button');
      clickFavoriteButtonAndWait();
      cy.log('Favorite button clicked and API call completed');

      // THEN: Favorite icon should show filled state (favorite)
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

      // GIVEN: User navigates to PAC_MAN file via deep link and favorites it first
      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      // Wait for UI to be fully loaded and favorite the file first
      waitForPlayerToolbarVisible();
      waitForFavoriteButtonReady();

      // WHEN: User clicks favorite button again to unfavorite
      cy.log('About to click favorite button to unfavorite');
      clickFavoriteButtonAndWaitForRemove();
      cy.log('Favorite button clicked and remove API call completed');

      // THEN: Favorite icon should show empty state (favorite_border)
      verifyFavoriteIconIsEmpty();
    });
  });

  describe('File Disappears from Favorites Directory After Unfavoriting', () => {
    it('should remove PAC_MAN from favorites directory when unfavorited from player toolbar', () => {
      // Setup: Pre-favorite PAC_MAN so it appears in favorites directory
      filesystem.saveFavorite(TEST_FILES.GAMES.PAC_MAN.filePath);

      interceptGetDirectory({ filesystem });
      interceptLaunchFile({ filesystem });
      interceptRemoveFavorite({ filesystem });

      // GIVEN: User launches PAC_MAN from favorites directory and file is currently playing
      launchFileFromFavorites({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        fileName: TEST_FILES.GAMES.PAC_MAN.fileName,
      });
      waitForPlayerToolbarVisible();
      waitForFavoriteButtonReady();     
      verifyFavoriteIconIsFilled();

      // WHEN: User clicks favorite button to unfavorite (while file is playing)      
      clickFavoriteButtonAndWaitForRemove();      

      // THEN: File disappears from favorites directory listing
       cy.log('Verifying PAC_MAN disappeared from favorites directory');
      const favoritesFilePath = `${TEST_PATHS.FAVORITES_GAMES}/${TEST_FILES.GAMES.PAC_MAN.fileName}`;
      verifyFileNotInDirectory(favoritesFilePath);

      // AND: verify the favorite icon is now empty
      verifyFavoriteIconIsEmpty();
    });
  });

  describe('API Error Handling and User Feedback', () => {
    it('should display error alert when save favorite operation fails', () => {
      // Setup: Configure save favorite error interceptor
      setupSaveFavoriteErrorScenario(filesystem);

      interceptGetDirectory({ filesystem });
      interceptLaunchFile({ filesystem });

      // GIVEN: User is viewing PAC_MAN with save favorite error interceptor
      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      // Wait for UI to be fully loaded
      waitForPlayerToolbarVisible();
      waitForFavoriteButtonReady();

      // WHEN: User clicks favorite button and API call fails
      cy.log('About to click favorite button expecting error');
      clickFavoriteButton();
      cy.wait('@saveFavorite');

      // THEN: Error alert appears with failure message
      verifyErrorAlertDisplayed('Bad Request');

      // AND: Favorite state remains unchanged (still shows empty heart)
      verifyFavoriteStateUnchangedAfterError('favorite_border');

      // AND: Favorite button is enabled after error
      verifyFavoriteButtonEnabledAfterError();
    });

    it('should display error alert when remove favorite operation fails', () => {
      // Setup: Pre-favorite PAC_MAN and configure remove favorite error interceptor
      filesystem.saveFavorite(TEST_FILES.GAMES.PAC_MAN.filePath);
      setupRemoveFavoriteErrorScenario(filesystem);

      interceptGetDirectory({ filesystem });
      interceptLaunchFile({ filesystem });

      // GIVEN: User is viewing a favorited PAC_MAN with remove favorite error interceptor
      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      // Wait for UI to be fully loaded
      waitForPlayerToolbarVisible();
      waitForFavoriteButtonReady();

      // WHEN: User clicks favorite button and API call fails
      cy.log('About to click favorite button expecting remove error');
      clickFavoriteButton();
      cy.wait('@removeFavorite');

      // THEN: Error alert appears with failure message
      verifyErrorAlertDisplayed('Bad Request');

      // AND: Favorite state remains unchanged (still shows filled heart)
      verifyFavoriteStateUnchangedAfterError('favorite');

      // AND: Favorite button is enabled after error
      verifyFavoriteButtonEnabledAfterError();
    });
  });
});