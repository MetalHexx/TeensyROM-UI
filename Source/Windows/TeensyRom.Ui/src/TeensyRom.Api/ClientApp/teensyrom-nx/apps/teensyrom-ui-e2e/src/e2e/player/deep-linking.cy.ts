import type MockFilesystem from '../../support/test-data/mock-filesystem/mock-filesystem';
import { createMockFilesystem } from '../../support/test-data/generators/storage.generators';
import { singleDevice } from '../../support/test-data/fixtures';
import { interceptConnectDevice } from '../../support/interceptors/connectDevice.interceptors';
import { interceptFindDevices } from '../../support/interceptors/findDevices.interceptors';
import { interceptGetDirectory } from '../../support/interceptors/getDirectory.interceptors';
import { interceptLaunchFile } from '../../support/interceptors/launchFile.interceptors';
import { interceptLaunchRandom } from '../../support/interceptors/launchRandom.interceptors';
import { DIRECTORY_FILES_SELECTORS, PLAYER_TOOLBAR_SELECTORS } from '../../support/constants/selector.constants';
import { VIEWPORT, TIMEOUTS, MOCK_SEEDS } from '../../support/constants/test.constants';
import { TeensyStorageType, TEST_PATHS, TEST_FILES } from '../../support/constants/storage.constants';
import {
  navigateToPlayer,
  waitForFileToLoad,
  waitForRandomLaunch,
  waitForDirectoryFilesToBeVisible,
  waitForFileInfoToAppear,
  waitForFileLaunch,
  verifyFileInDirectory,
  expectNoFileLaunched,
  expectFileIsLaunched,
  expectPlayerToolbarVisible,
  expectUrlContainsParams,
  doubleClickFileInDirectory,
  clickNextButton,
  clickRandomButton,
  goBack,
  goForward,
  verifyAlertVisible,
  verifyAlertMessage,
  verifyAlertSeverity,
  ALERT_SEVERITY,
} from './test-helpers';

describe('Deep Linking', () => {
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
    interceptGetDirectory({ filesystem });
    interceptLaunchFile({ filesystem });
  });

  describe('Route Resolution & Navigation', () => {
    it('navigates to directory without launching file', () => {

      // Navigate to directory without file parameter
      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
      });

      // Wait for the directory files to actually appear in the DOM
      // The app makes multiple directory calls on load (USB root, SD root, then SD /games)
      // We need to wait for the /games directory to load, not just the first directory call

      // Wait for the directory files component to render
      cy.get(DIRECTORY_FILES_SELECTORS.directoryFilesContainer, { timeout: TIMEOUTS.DEFAULT }).should('exist');

      // Wait for actual file items to be visible
      cy.get(DIRECTORY_FILES_SELECTORS.fileListItemsByPrefix(TEST_PATHS.GAMES + '/'), { timeout: TIMEOUTS.DEFAULT })
        .first()
        .should('be.visible');

      // Verify directory loaded
      verifyFileInDirectory(TEST_FILES.GAMES.PAC_MAN.filePath, true);
      verifyFileInDirectory(TEST_FILES.GAMES.DONKEY_KONG.filePath, true);

      // Verify no file launched
      expectNoFileLaunched();

      // Verify URL unchanged
      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
      });
    });

    it('auto-launches file when file parameter provided', () => {

      // Setup file launch interceptor for this test
      interceptLaunchFile({ filesystem });
      interceptGetDirectory({ filesystem });

      // Navigate with all 4 parameters
      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      waitForFileToLoad();

      // Verify file launched (file info displays title "Pac-Man", not full filename)
      expectFileIsLaunched(TEST_FILES.GAMES.PAC_MAN.title);
      expectPlayerToolbarVisible();

      // Verify URL contains all parameters
      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });
    });

    it('displays default view when no parameters provided', () => {

      // Navigate to player with no parameters
      navigateToPlayer();

      // Wait for the directory files component to render (indicates app loaded)
      cy.get(DIRECTORY_FILES_SELECTORS.directoryFilesContainer, { timeout: TIMEOUTS.DEFAULT }).should('exist');

      // Verify no deep linking occurred
      expectNoFileLaunched();

      // Verify URL has no parameters
      cy.location('search').should('eq', '');
    });
  });

  describe('URL Updates on User Actions', () => {
    it('updates URL when file clicked from directory', () => {

      // Setup file launch interceptor for this test
      interceptLaunchFile({ filesystem });

      // Start at directory view
      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
      });

      // Wait for directory files to be visible
      waitForDirectoryFilesToBeVisible(TEST_PATHS.GAMES + '/');

      // Double-click the file to launch it
      doubleClickFileInDirectory(TEST_FILES.GAMES.PAC_MAN.filePath);

      // Wait for file info to appear (indicates file loaded)
      waitForFileInfoToAppear();

      // Verify file launched (file info displays title "Pac-Man")
      expectFileIsLaunched(TEST_FILES.GAMES.PAC_MAN.title);

      // Verify URL updated with file parameter
      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      }, { timeout: TIMEOUTS.DEFAULT });
    });

    it('updates URL when next file button clicked', () => {
      // Setup file launch interceptor for this test
      interceptLaunchFile({ filesystem });
      interceptGetDirectory({ filesystem });

      // Start with file playing
      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      waitForFileToLoad();

      // Click next button
      clickNextButton();
      waitForFileInfoToAppear();

      // Verify URL updated with all parameters
      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
      });

      // Verify a file parameter exists but changed from Pac-Man
      cy.location('search').should((search) => {
        expect(search).to.include('file=');
        expect(search).to.not.include('Pac-Man');
      });

      // Verify browser history updated
      goBack();
      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });
    });

    it('updates URL when random file button clicked', () => {

      // Setup random launch interceptor
      interceptLaunchRandom();

      // Navigate to player with no parameters
      navigateToPlayer();

      // Click random button
      clickRandomButton();

      // Wait for random launch to complete
      waitForRandomLaunch();

      // Verify URL was updated with file parameter
      cy.location('search').should((search) => {
        expect(search).to.include('file=');
      });
    });
  });

  describe('Browser History Navigation', () => {
    it('relaunches files when navigating back and forward', () => {

      const gamesDirectory = filesystem.getDirectory(TEST_PATHS.GAMES);
      const donkeyKongFile = gamesDirectory.storageItem.files?.find(
        (f) => f.name === TEST_FILES.GAMES.DONKEY_KONG.fileName
      );

      interceptLaunchFile({ filesystem });
      interceptLaunchRandom({ selectedFile: donkeyKongFile });
      interceptGetDirectory({ filesystem });

      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      waitForFileToLoad();
      waitForFileLaunch();
      expectFileIsLaunched(TEST_FILES.GAMES.PAC_MAN.title);

      clickRandomButton();
      waitForRandomLaunch();

      // Wait specifically for the Donkey Kong file info to appear
      cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo, { timeout: 10000 })
        .should('be.visible')
        .and('contain.text', TEST_FILES.GAMES.DONKEY_KONG.title);

      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.DONKEY_KONG.fileName,
      }, { timeout: 10000, logWaiting: true });

      goBack();
      waitForFileLaunch();

      // Wait specifically for the Pac-Man file info to appear
      cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo, { timeout: 10000 })
        .should('be.visible')
        .and('contain.text', TEST_FILES.GAMES.PAC_MAN.title);

      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      }, { timeout: 10000, logWaiting: true });

      goForward();
      waitForFileLaunch();

      // Wait specifically for the Donkey Kong file info to appear again
      cy.get(PLAYER_TOOLBAR_SELECTORS.currentFileInfo, { timeout: 10000 })
        .should('be.visible')
        .and('contain.text', TEST_FILES.GAMES.DONKEY_KONG.title);

      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.DONKEY_KONG.fileName,
      }, { timeout: 10000, logWaiting: true });
    });
  });

  describe('Deep Linking Error Handling', () => {
    it('displays warning alert when file parameter does not match any file in directory', () => {
      const missingFile = 'MissingFile.sid';

      // Navigate with invalid file parameter
      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: missingFile,
      });

      // Verify warning alert is displayed to the user
      verifyAlertVisible();
      verifyAlertMessage(`File "${missingFile}" not found in directory "${TEST_PATHS.GAMES}"`);
      verifyAlertSeverity(ALERT_SEVERITY.WARNING);

      // No file should be launched when file parameter is invalid
      expectNoFileLaunched();
    });

    it('displays warning alert when directory path is invalid', () => {
      const invalidPath = '/invalid/path';
      const requestedFile = 'GhostsAndGoblins.crt';

      // Force directory load to fail
      interceptGetDirectory({ errorMode: true });

      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: invalidPath,
        file: requestedFile,
      });

      verifyAlertVisible();
      verifyAlertMessage(`Directory "${invalidPath}" could not be loaded or has no files`);
      verifyAlertSeverity(ALERT_SEVERITY.WARNING);
    });
  });
});
