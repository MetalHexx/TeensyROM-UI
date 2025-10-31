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

    filesystem = createMockFilesystem(MOCK_SEEDS.DEFAULT);
    testDeviceId = singleDevice.devices[0].deviceId;

    interceptFindDevices({ fixture: singleDevice });
    interceptConnectDevice();
    interceptGetDirectory({ filesystem });
    interceptLaunchFile({ filesystem });
  });

  describe('Route Resolution & Navigation', () => {
    it('navigates to directory without launching file', () => {
      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
      });

      // Wait for directory files component to render and files to be visible
      // The app makes multiple directory calls on load (USB root, SD root, then SD /games)
      cy.get(DIRECTORY_FILES_SELECTORS.directoryFilesContainer, { timeout: TIMEOUTS.DEFAULT }).should('exist');

      cy.get(DIRECTORY_FILES_SELECTORS.fileListItemsByPrefix(TEST_PATHS.GAMES + '/'), { timeout: TIMEOUTS.DEFAULT })
        .first()
        .should('be.visible');

      verifyFileInDirectory(TEST_FILES.GAMES.PAC_MAN.filePath, true);
      verifyFileInDirectory(TEST_FILES.GAMES.DONKEY_KONG.filePath, true);

      expectNoFileLaunched();

      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
      });
    });

    it('auto-launches file when file parameter provided', () => {
      interceptLaunchFile({ filesystem });
      interceptGetDirectory({ filesystem });

      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      waitForFileToLoad();

      expectFileIsLaunched(TEST_FILES.GAMES.PAC_MAN.title);
      expectPlayerToolbarVisible();

      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });
    });

    it('displays default view when no parameters provided', () => {
      navigateToPlayer();

      cy.get(DIRECTORY_FILES_SELECTORS.directoryFilesContainer, { timeout: TIMEOUTS.DEFAULT }).should('exist');

      expectNoFileLaunched();

      cy.location('search').should('eq', '');
    });
  });

  describe('URL Updates on User Actions', () => {
    it('updates URL when file clicked from directory', () => {
      interceptLaunchFile({ filesystem });

      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
      });

      waitForDirectoryFilesToBeVisible(TEST_PATHS.GAMES + '/');

      doubleClickFileInDirectory(TEST_FILES.GAMES.PAC_MAN.filePath);

      waitForFileInfoToAppear();

      expectFileIsLaunched(TEST_FILES.GAMES.PAC_MAN.title);

      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      }, { timeout: TIMEOUTS.DEFAULT });
    });

    it('updates URL when next file button clicked', () => {
      interceptLaunchFile({ filesystem });
      interceptGetDirectory({ filesystem });

      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });

      waitForFileToLoad();

      clickNextButton();
      waitForFileInfoToAppear();

      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
      });

      cy.location('search').should((search) => {
        expect(search).to.include('file=');
        expect(search).to.not.include('Pac-Man');
      });

      goBack();
      expectUrlContainsParams({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: TEST_FILES.GAMES.PAC_MAN.fileName,
      });
    });

    it('updates URL when random file button clicked', () => {
      interceptLaunchRandom();

      navigateToPlayer();

      clickRandomButton();

      waitForRandomLaunch();

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

      navigateToPlayer({
        device: testDeviceId,
        storage: TeensyStorageType.Sd,
        path: TEST_PATHS.GAMES,
        file: missingFile,
      });

      verifyAlertVisible();
      verifyAlertMessage(`File "${missingFile}" not found in directory "${TEST_PATHS.GAMES}"`);
      verifyAlertSeverity(ALERT_SEVERITY.WARNING);

      expectNoFileLaunched();
    });

    it('displays warning alert when directory path is invalid', () => {
      const invalidPath = '/invalid/path';
      const requestedFile = 'GhostsAndGoblins.crt';

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
