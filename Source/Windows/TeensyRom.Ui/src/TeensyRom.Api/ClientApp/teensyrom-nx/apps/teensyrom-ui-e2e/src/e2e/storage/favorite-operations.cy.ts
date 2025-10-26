import type MockFilesystem from '../../support/test-data/mock-filesystem/mock-filesystem';
import { createMockFilesystem } from '../../support/test-data/generators/storage.generators';
import { singleDevice } from '../../support/test-data/fixtures';
import {
  interceptConnectDevice,
  interceptFindDevices,
  interceptGetDirectory,
  interceptRemoveFavorite,
  interceptSaveFavorite,
} from '../../support/interceptors';
import {
  clickFavoriteButton,
  expectFavoriteButtonDisabled,
  expectFavoriteIcon,
  loadFileInPlayer,
  openFavoritesDirectory,
  verifyAlert,
  verifyFileInDirectory,
  waitForDirectoryLoad,
  waitForRemoveFavorite,
  waitForSaveFavorite,
} from './test-helpers';

describe('Favorite Operations', () => {
  let filesystem: MockFilesystem;

  beforeEach(() => {
    filesystem = createMockFilesystem(12345);

    interceptFindDevices({ fixture: singleDevice });
    interceptConnectDevice();
    interceptGetDirectory({ filesystem });
    interceptSaveFavorite({ filesystem });
    interceptRemoveFavorite({ filesystem });

    loadFileInPlayer('/games/Pac-Man (J1).crt');
    waitForDirectoryLoad();
  });

  it('saves a favorite and displays it in the favorites directory', () => {
    expectFavoriteIcon('favorite_border');

    clickFavoriteButton();
    waitForSaveFavorite();

    expectFavoriteIcon('favorite');
    verifyAlert('Favorite tagged and saved to /favorites/games');

    openFavoritesDirectory('games');
    waitForDirectoryLoad();
    verifyFileInDirectory('Pac-Man (J1).crt', true);
  });

  it('recovers from a save favorite failure and succeeds on retry', () => {
    interceptSaveFavorite({ filesystem, errorMode: true });

    clickFavoriteButton();
    waitForSaveFavorite();

    expectFavoriteIcon('favorite_border');
    verifyAlert('Failed to save favorite. Please try again.');

    interceptSaveFavorite({ filesystem });

    clickFavoriteButton();
    waitForSaveFavorite();

    expectFavoriteIcon('favorite');
    verifyAlert('Favorite tagged and saved to /favorites/games');
  });

  it('disables the favorite button while a save operation is in progress', () => {
    interceptSaveFavorite({ filesystem, responseDelayMs: 500 });

    clickFavoriteButton();
    expectFavoriteButtonDisabled(true);

    waitForSaveFavorite();

    expectFavoriteButtonDisabled(false);
    expectFavoriteIcon('favorite');
  });

  it('removes a favorite and synchronizes the favorites directory', () => {
    filesystem.saveFavorite('/games/Pac-Man (J1).crt');

    loadFileInPlayer('/games/Pac-Man (J1).crt');
    waitForDirectoryLoad();
    expectFavoriteIcon('favorite');

    openFavoritesDirectory('games');
    waitForDirectoryLoad();
    verifyFileInDirectory('Pac-Man (J1).crt', true);

    clickFavoriteButton();
    waitForRemoveFavorite();

    expectFavoriteIcon('favorite_border');
    verifyAlert('Favorite untagged and removed from /favorites/games');

    openFavoritesDirectory('games');
    waitForDirectoryLoad();
    verifyFileInDirectory('Pac-Man (J1).crt', false);
  });

  it('favorites multiple file types and lists them by category', () => {
    expectFavoriteIcon('favorite_border');

    clickFavoriteButton();
    waitForSaveFavorite();
    verifyAlert('Favorite tagged and saved to /favorites/games');

    loadFileInPlayer('/music/MUSICIANS/L/LukHash/Alpha.sid');
    waitForDirectoryLoad();
    expectFavoriteIcon('favorite_border');

    clickFavoriteButton();
    waitForSaveFavorite();
    verifyAlert('Favorite tagged and saved to /favorites/music');

    loadFileInPlayer('/images/SonicTheHedgehog.kla');
    waitForDirectoryLoad();
    expectFavoriteIcon('favorite_border');

    clickFavoriteButton();
    waitForSaveFavorite();
    verifyAlert('Favorite tagged and saved to /favorites/images');

    openFavoritesDirectory('games');
    waitForDirectoryLoad();
    verifyFileInDirectory('Pac-Man (J1).crt', true);

    openFavoritesDirectory('music');
    waitForDirectoryLoad();
    verifyFileInDirectory('Alpha.sid', true);

    openFavoritesDirectory('images');
    waitForDirectoryLoad();
    verifyFileInDirectory('SonicTheHedgehog.kla', true);
  });
});
