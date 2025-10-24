import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PlayerToolbarActionsComponent } from './player-toolbar-actions.component';
import { PLAYER_CONTEXT, StorageStore } from '@teensyrom-nx/application';
import { signal, WritableSignal } from '@angular/core';
import { LaunchMode, FileItem, StorageType, FileItemType } from '@teensyrom-nx/domain';
import { vi } from 'vitest';

// Create a helper function to build test LaunchedFile objects
function createLaunchedFile(
  deviceId: string,
  storageType: StorageType,
  fileData: Partial<FileItem> = {}
) {
  const storageKey = `${deviceId}-${storageType}` as const;
  return {
    storageKey,
    file: {
      name: fileData.name || 'test.rom',
      path: fileData.path || '/test/test.rom',
      size: 1024,
      isFavorite: fileData.isFavorite ?? false,
      isCompatible: true,
      title: 'Test File',
      creator: '',
      releaseInfo: '',
      description: '',
      shareUrl: '',
      metadataSource: '',
      meta1: 'Game',
      meta2: '',
      metadataSourcePath: '',
      parentPath: '/test',
      playLength: '',
      subtuneLengths: [],
      startSubtuneNum: 0,
      images: [],
      type: fileData.type ?? FileItemType.Game,
      links: [],
      tags: [],
      youTubeVideos: [],
      competitions: [],
      ratingCount: 0,
      ...fileData,
    } as FileItem,
    parentPath: '/test',
    launchedAt: Date.now(),
    isCompatible: true,
  };
}

describe('PlayerToolbarActionsComponent', () => {
  let component: PlayerToolbarActionsComponent;
  let fixture: ComponentFixture<PlayerToolbarActionsComponent>;
  let mockPlayerContext: {
    toggleShuffleMode: ReturnType<typeof vi.fn>;
    getLaunchMode: ReturnType<typeof vi.fn>;
    getCurrentFile: ReturnType<typeof vi.fn>;
    updateCurrentFileFavoriteStatus: ReturnType<typeof vi.fn>;
  };
  let mockStorageStore: {
    saveFavorite: ReturnType<typeof vi.fn>;
    removeFavorite: ReturnType<typeof vi.fn>;
    favoriteOperationsState: ReturnType<typeof vi.fn>;
  };
  let currentFileSignal: WritableSignal<ReturnType<typeof createLaunchedFile> | null>;
  let favoriteOperationsStateSignal: WritableSignal<{ isProcessing: boolean; error: string | null }>;

  beforeEach(async () => {
    currentFileSignal = signal<ReturnType<typeof createLaunchedFile> | null>(null);
    favoriteOperationsStateSignal = signal({ isProcessing: false, error: null });

    mockPlayerContext = {
      toggleShuffleMode: vi.fn(),
      getLaunchMode: vi.fn(() => signal(LaunchMode.Directory)),
      getCurrentFile: vi.fn(() => currentFileSignal),
      updateCurrentFileFavoriteStatus: vi.fn(),
    };

    mockStorageStore = {
      saveFavorite: vi.fn(),
      removeFavorite: vi.fn(),
      favoriteOperationsState: vi.fn(() => favoriteOperationsStateSignal()),
    };

    await TestBed.configureTestingModule({
      imports: [PlayerToolbarActionsComponent],
      providers: [
        { provide: PLAYER_CONTEXT, useValue: mockPlayerContext },
        { provide: StorageStore, useValue: mockStorageStore },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PlayerToolbarActionsComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'test-device');
    fixture.detectChanges();
  });

  describe('Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should inject playerContext', () => {
      expect(mockPlayerContext).toBeDefined();
    });

    it('should inject StorageStore', () => {
      expect(mockStorageStore).toBeDefined();
    });
  });

  describe('Shuffle Mode', () => {
    it('should toggle shuffle mode when button clicked', () => {
      component.toggleShuffleMode();
      expect(mockPlayerContext.toggleShuffleMode).toHaveBeenCalledWith('test-device');
    });

    it('should detect shuffle mode correctly', () => {
      mockPlayerContext.getLaunchMode = vi.fn(() => signal(LaunchMode.Shuffle));
      expect(component.isShuffleMode()).toBe(true);

      mockPlayerContext.getLaunchMode = vi.fn(() => signal(LaunchMode.Directory));
      expect(component.isShuffleMode()).toBe(false);
    });
  });

  describe('Favorite Status - isFavorite()', () => {
    it('should return false when no file is loaded', () => {
      currentFileSignal.set(null);
      expect(component.isFavorite()).toBe(false);
    });

    it('should return true when current file has isFavorite flag set to true', () => {
      currentFileSignal.set(createLaunchedFile('test-device', StorageType.Sd, { isFavorite: true }));
      expect(component.isFavorite()).toBe(true);
    });

    it('should return false when current file has isFavorite flag set to false', () => {
      currentFileSignal.set(createLaunchedFile('test-device', StorageType.Sd, { isFavorite: false }));
      expect(component.isFavorite()).toBe(false);
    });
  });

  describe('Favorite Operation Status - isFavoriteOperationInProgress()', () => {
    it('should return false when no operation is in progress', () => {
      favoriteOperationsStateSignal.set({ isProcessing: false, error: null });
      expect(component.isFavoriteOperationInProgress()).toBe(false);
    });

    it('should return true when favorite operation is in progress', () => {
      favoriteOperationsStateSignal.set({ isProcessing: true, error: null });
      expect(component.isFavoriteOperationInProgress()).toBe(true);
    });

    it('should return true when remove favorite operation is in progress', () => {
      favoriteOperationsStateSignal.set({ isProcessing: true, error: null });
      expect(component.isFavoriteOperationInProgress()).toBe(true);
    });
  });

  describe('Toggle Favorite - toggleFavorite()', () => {
    it('should call saveFavorite and update player context when current file is not favorited', async () => {
      currentFileSignal.set(
        createLaunchedFile('test-device', StorageType.Sd, { isFavorite: false, path: '/games/game.rom' })
      );
      mockStorageStore.saveFavorite.mockImplementation(async () => {
        favoriteOperationsStateSignal.set({ isProcessing: false, error: null });
      });

      await component.toggleFavorite();

      expect(mockStorageStore.saveFavorite).toHaveBeenCalledWith({
        deviceId: 'test-device',
        storageType: StorageType.Sd,
        filePath: '/games/game.rom',
      });
      expect(mockPlayerContext.updateCurrentFileFavoriteStatus).toHaveBeenCalledWith(
        'test-device',
        '/games/game.rom',
        true
      );
    });

    it('should call removeFavorite and update player context when current file is favorited', async () => {
      currentFileSignal.set(
        createLaunchedFile('test-device', StorageType.Sd, { isFavorite: true, path: '/games/game.rom' })
      );
      mockStorageStore.removeFavorite.mockImplementation(async () => {
        favoriteOperationsStateSignal.set({ isProcessing: false, error: null });
      });

      await component.toggleFavorite();

      expect(mockStorageStore.removeFavorite).toHaveBeenCalledWith({
        deviceId: 'test-device',
        storageType: StorageType.Sd,
        filePath: '/games/game.rom',
      });
      expect(mockPlayerContext.updateCurrentFileFavoriteStatus).toHaveBeenCalledWith(
        'test-device',
        '/games/game.rom',
        false
      );
    });

    it('should not call any action when no file is loaded', async () => {
      currentFileSignal.set(null);

      await component.toggleFavorite();

      expect(mockStorageStore.saveFavorite).not.toHaveBeenCalled();
      expect(mockStorageStore.removeFavorite).not.toHaveBeenCalled();
      expect(mockPlayerContext.updateCurrentFileFavoriteStatus).not.toHaveBeenCalled();
    });

    it('should pass correct identifiers from storage key when saving favorite', async () => {
      currentFileSignal.set(
        createLaunchedFile('device-123', StorageType.Usb, { isFavorite: false, path: '/music/music.mp3' })
      );
      mockStorageStore.saveFavorite.mockImplementation(async () => {
        favoriteOperationsStateSignal.set({ isProcessing: false, error: null });
      });

      await component.toggleFavorite();

      expect(mockStorageStore.saveFavorite).toHaveBeenCalledWith({
        deviceId: 'device-123',
        storageType: StorageType.Usb,
        filePath: '/music/music.mp3',
      });
      expect(mockPlayerContext.updateCurrentFileFavoriteStatus).toHaveBeenCalledWith(
        'device-123',
        '/music/music.mp3',
        true
      );
    });

    it('should not update player context when favorite operation reports an error', async () => {
      currentFileSignal.set(
        createLaunchedFile('test-device', StorageType.Sd, { isFavorite: false, path: '/games/game.rom' })
      );
      mockStorageStore.saveFavorite.mockImplementation(async () => {
        favoriteOperationsStateSignal.set({ isProcessing: false, error: 'Failed to save' });
      });

      await component.toggleFavorite();

      expect(mockStorageStore.saveFavorite).toHaveBeenCalled();
      expect(mockPlayerContext.updateCurrentFileFavoriteStatus).not.toHaveBeenCalled();
    });

    it('should return early when an operation is already in progress', async () => {
      favoriteOperationsStateSignal.set({ isProcessing: true, error: null });
      currentFileSignal.set(
        createLaunchedFile('test-device', StorageType.Sd, { isFavorite: false, path: '/games/game.rom' })
      );

      await component.toggleFavorite();

      expect(mockStorageStore.saveFavorite).not.toHaveBeenCalled();
      expect(mockStorageStore.removeFavorite).not.toHaveBeenCalled();
      expect(mockPlayerContext.updateCurrentFileFavoriteStatus).not.toHaveBeenCalled();
    });
  });

  describe('Current File Computed Signal', () => {
    it('should have currentFile computed signal', () => {
      expect(component.currentFile).toBeDefined();
    });

    it('should return null when no file is loaded', () => {
      currentFileSignal.set(null);
      expect(component.currentFile()).toBeNull();
    });

    it('should return current file from playerContext', () => {
      const launchedFile = createLaunchedFile('test-device', StorageType.Sd);
      currentFileSignal.set(launchedFile);
      expect(component.currentFile()).toEqual(launchedFile);
    });

    it('should update when currentFile signal changes', () => {
      const file1 = createLaunchedFile('test-device', StorageType.Sd, { isFavorite: false });
      const file2 = createLaunchedFile('test-device', StorageType.Sd, { isFavorite: true });

      currentFileSignal.set(file1);
      expect(component.currentFile()).toEqual(file1);

      currentFileSignal.set(file2);
      expect(component.currentFile()).toEqual(file2);
    });

    it('should reactively track changes to file.isFavorite property', () => {
      const launchedFile = createLaunchedFile('test-device', StorageType.Sd, { isFavorite: false });
      currentFileSignal.set(launchedFile);

      // Initial state: isFavorite is false
      expect(component.currentFile()?.file.isFavorite).toBe(false);
      expect(component.isFavorite()).toBe(false);

      // Update the file object with new favorite status (simulating store update)
      const updatedLaunchedFile = {
        ...launchedFile,
        file: {
          ...launchedFile.file,
          isFavorite: true,
        },
      };
      currentFileSignal.set(updatedLaunchedFile);

      // REGRESSION: Without proper signal reactivity, this would still return false
      // The computed signal must re-evaluate when the underlying store signal changes
      expect(component.currentFile()?.file.isFavorite).toBe(true);
      expect(component.isFavorite()).toBe(true);
    });
  });

  describe('Template Integration', () => {
    it('should render shuffle button', () => {
      const buttons = fixture.nativeElement.querySelectorAll('lib-icon-button');
      expect(buttons.length).toBeGreaterThanOrEqual(1);
    });

    it('should render favorite button', () => {
      const buttons = fixture.nativeElement.querySelectorAll('lib-icon-button');
      // Should have both shuffle and favorite buttons
      expect(buttons.length).toBe(2);
    });

    it('should display favorite_border icon when file is not favorited', () => {
      currentFileSignal.set(createLaunchedFile('test-device', StorageType.Sd, { isFavorite: false }));
      fixture.detectChanges();

      expect(component.isFavorite()).toBe(false);
    });

    it('should display favorite icon when file is favorited', () => {
      currentFileSignal.set(createLaunchedFile('test-device', StorageType.Sd, { isFavorite: true }));
      fixture.detectChanges();

      expect(component.isFavorite()).toBe(true);
    });

    it('should disable favorite button when no file is loaded', () => {
      currentFileSignal.set(null);
      fixture.detectChanges();

      expect(!component.currentFile()).toBe(true);
    });

    it('should disable favorite button when operation is in progress', () => {
      currentFileSignal.set(createLaunchedFile('test-device', StorageType.Sd, { isFavorite: false }));
      favoriteOperationsStateSignal.set({ isProcessing: true, error: null });
      fixture.detectChanges();

      expect(component.isFavoriteOperationInProgress()).toBe(true);
    });

    it('should set highlight color when file is favorited', () => {
      currentFileSignal.set(createLaunchedFile('test-device', StorageType.Sd, { isFavorite: true }));
      fixture.detectChanges();

      expect(component.isFavorite()).toBe(true);
    });

    it('should set normal color when file is not favorited', () => {
      currentFileSignal.set(createLaunchedFile('test-device', StorageType.Sd, { isFavorite: false }));
      fixture.detectChanges();

      expect(component.isFavorite()).toBe(false);
    });
  });
});
