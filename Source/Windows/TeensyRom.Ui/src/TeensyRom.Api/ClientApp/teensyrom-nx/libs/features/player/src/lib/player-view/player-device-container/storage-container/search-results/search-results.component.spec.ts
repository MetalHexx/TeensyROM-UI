import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal, WritableSignal, Signal } from '@angular/core';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { SearchResultsComponent } from './search-results.component';
import {
  PLAYER_CONTEXT,
  SearchState,
  LaunchedFile,
  StorageKeyUtil,
  IPlayerContext,
  StorageStore,
  StorageDirectoryState,
  ShuffleSettings,
} from '@teensyrom-nx/application';
import {
  FileItem,
  FileItemType,
  LaunchMode,
  StorageType,
  PlayerFilterType,
  PlayerScope,
} from '@teensyrom-nx/domain';

describe('SearchResultsComponent', () => {
  let component: SearchResultsComponent;
  let fixture: ComponentFixture<SearchResultsComponent>;
  let mockStorageStore: {
    getSearchState: (deviceId: string, storageType: StorageType) => Signal<SearchState | null>;
    getSelectedDirectoryState: (deviceId: string) => Signal<StorageDirectoryState | null>;
  };
  let mockPlayerContext: Partial<IPlayerContext>;

  let searchStateSignal: WritableSignal<SearchState | null>;
  let currentFileSignal: WritableSignal<LaunchedFile | null>;
  let launchModeSignal: WritableSignal<LaunchMode>;
  let errorSignal: WritableSignal<string | null>;
  let selectedDirectorySignal: WritableSignal<StorageDirectoryState | null>;
  let shuffleSettingsSignal: WritableSignal<ShuffleSettings | null>;

  const mockFileItem1: FileItem = {
    path: '/test/file1.sid',
    name: 'file1.sid',
    size: 1024,
    type: FileItemType.Song,
    images: [],
    parentPath: '/test',
    description: 'Test file 1',
    isFavorite: false,
    isCompatible: true,
    title: '',
    creator: '',
    releaseInfo: '',
    shareUrl: '',
    metadataSource: '',
    meta1: '',
    meta2: '',
    metadataSourcePath: '',
    playLength: '',
    subtuneLengths: [],
    startSubtuneNum: 0,
  };

  const mockFileItem2: FileItem = {
    path: '/test/file2.sid',
    name: 'file2.sid',
    size: 2048,
    type: FileItemType.Song,
    images: [],
    parentPath: '/test',
    description: 'Test file 2',
    isFavorite: false,
    isCompatible: true,
    title: '',
    creator: '',
    releaseInfo: '',
    shareUrl: '',
    metadataSource: '',
    meta1: '',
    meta2: '',
    metadataSourcePath: '',
    playLength: '',
    subtuneLengths: [],
    startSubtuneNum: 0,
  };

  beforeEach(async () => {
    // Create signal mocks
    searchStateSignal = signal<SearchState | null>(null);
    currentFileSignal = signal<LaunchedFile | null>(null);
    launchModeSignal = signal<LaunchMode>(LaunchMode.Directory);
    errorSignal = signal<string | null>(null);
    selectedDirectorySignal = signal<StorageDirectoryState | null>(null);
    shuffleSettingsSignal = signal<ShuffleSettings | null>({
      filter: PlayerFilterType.All,
      scope: PlayerScope.DirectoryShallow,
    });

    // Create mock StorageStore
    mockStorageStore = {
      getSearchState: () => searchStateSignal.asReadonly(),
      getSelectedDirectoryState: () => selectedDirectorySignal.asReadonly(),
    };

    // Create mock PlayerContext using IPlayerContext interface
    mockPlayerContext = {
      getCurrentFile: () => currentFileSignal.asReadonly(),
      getLaunchMode: () => launchModeSignal.asReadonly(),
      getError: () => errorSignal.asReadonly(),
      getShuffleSettings: () => shuffleSettingsSignal.asReadonly(),
      launchFileWithContext: vi.fn(),
    } as Partial<IPlayerContext>;

    await TestBed.configureTestingModule({
      imports: [SearchResultsComponent],
      providers: [
        provideNoopAnimations(),
        { provide: StorageStore, useValue: mockStorageStore },
        { provide: PLAYER_CONTEXT, useValue: mockPlayerContext },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SearchResultsComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'test-device');
  });

  describe('Component Rendering', () => {
    it('should create component successfully', () => {
      expect(component).toBeTruthy();
    });

    it('should display loading state when searching', () => {
      selectedDirectorySignal.set({
        deviceId: 'test-device',
        storageType: StorageType.Usb,
        currentPath: '/test',
        directory: null,
        isLoaded: true,
        isLoading: false,
        error: null,
        lastLoadTime: null,
      });

      searchStateSignal.set({
        searchText: 'test',
        filterType: null,
        results: [],
        isSearching: true,
        hasSearched: true, // Changed to true - search has been initiated
        error: null,
      });

      fixture.detectChanges();

      // When searching with no results yet, we shouldn't show empty state or results
      const emptyStateElement = fixture.nativeElement.querySelector('lib-empty-state-message');
      const resultsList = fixture.nativeElement.querySelector('.search-results-list');
      expect(emptyStateElement).toBeFalsy();
      expect(resultsList).toBeFalsy();
    });

    it('should display error state when search fails', () => {
      selectedDirectorySignal.set({
        deviceId: 'test-device',
        storageType: StorageType.Usb,
        currentPath: '/test',
        directory: null,
        isLoaded: true,
        isLoading: false,
        error: null,
        lastLoadTime: null,
      });

      searchStateSignal.set({
        searchText: 'test',
        filterType: null,
        results: [],
        isSearching: false,
        hasSearched: true,
        error: 'Search failed',
      });

      fixture.detectChanges();

      const errorElement = fixture.nativeElement.querySelector('.error-state');
      expect(errorElement).toBeTruthy();
      expect(errorElement?.textContent).toContain('Search failed');
    });

    it('should display "not searched" message initially', () => {
      selectedDirectorySignal.set({
        deviceId: 'test-device',
        storageType: StorageType.Usb,
        currentPath: '/test',
        directory: null,
        isLoaded: true,
        isLoading: false,
        error: null,
        lastLoadTime: null,
      });

      searchStateSignal.set({
        searchText: '',
        filterType: null,
        results: [],
        isSearching: false,
        hasSearched: false,
        error: null,
      });

      fixture.detectChanges();

      const emptyStateElement = fixture.nativeElement.querySelector('lib-empty-state-message');
      expect(emptyStateElement).toBeTruthy();
    });

    it('should display "no results" message for empty results', () => {
      selectedDirectorySignal.set({
        deviceId: 'test-device',
        storageType: StorageType.Usb,
        currentPath: '/test',
        directory: null,
        isLoaded: true,
        isLoading: false,
        error: null,
        lastLoadTime: null,
      });

      searchStateSignal.set({
        searchText: 'test',
        filterType: null,
        results: [],
        isSearching: false,
        hasSearched: true,
        error: null,
      });

      fixture.detectChanges();

      const emptyStateElement = fixture.nativeElement.querySelector('lib-empty-state-message');
      expect(emptyStateElement).toBeTruthy();
    });

    it('should render search results list', () => {
      selectedDirectorySignal.set({
        deviceId: 'test-device',
        storageType: StorageType.Usb,
        currentPath: '/test',
        directory: null,
        isLoaded: true,
        isLoading: false,
        error: null,
        lastLoadTime: null,
      });

      searchStateSignal.set({
        searchText: 'test',
        filterType: null,
        results: [mockFileItem1, mockFileItem2],
        isSearching: false,
        hasSearched: true,
        error: null,
      });

      fixture.detectChanges();

      const listItems = fixture.nativeElement.querySelectorAll('.file-list-item');
      expect(listItems.length).toBe(2);
    });
  });

  describe('File Selection', () => {
    beforeEach(() => {
      selectedDirectorySignal.set({
        deviceId: 'test-device',
        storageType: StorageType.Usb,
        currentPath: '/test',
        directory: null,
        isLoaded: true,
        isLoading: false,
        error: null,
        lastLoadTime: null,
      });

      searchStateSignal.set({
        searchText: 'test',
        filterType: null,
        results: [mockFileItem1, mockFileItem2],
        isSearching: false,
        hasSearched: true,
        error: null,
      });
      fixture.detectChanges();
    });

    it('should select file on single-click', () => {
      expect(component.selectedItem()).toBeNull();

      component.onFileSelected(mockFileItem1);

      expect(component.selectedItem()).toEqual(mockFileItem1);
    });

    it('should apply selected CSS class to selected file', () => {
      component.onFileSelected(mockFileItem1);
      fixture.detectChanges();

      // The lib-file-item component receives the [selected] input binding
      // Check that the component properly marks the file as selected
      expect(component.isSelected(mockFileItem1)).toBe(true);
      expect(component.isSelected(mockFileItem2)).toBe(false);
    });

    it('should launch file with Search mode on double-click', async () => {
      await component.onFileDoubleClick(mockFileItem1);

      expect(mockPlayerContext.launchFileWithContext).toHaveBeenCalledWith({
        deviceId: 'test-device',
        storageType: StorageType.Usb,
        file: mockFileItem1,
        directoryPath: '/test',
        files: [mockFileItem1, mockFileItem2],
        launchMode: LaunchMode.Search,
      });
    });

    it('should return true from isSelected() for selected file', () => {
      component.onFileSelected(mockFileItem1);

      expect(component.isSelected(mockFileItem1)).toBe(true);
      expect(component.isSelected(mockFileItem2)).toBe(false);
    });
  });

  describe('Player Integration', () => {
    beforeEach(() => {
      selectedDirectorySignal.set({
        deviceId: 'test-device',
        storageType: StorageType.Usb,
        currentPath: '/test',
        directory: null,
        isLoaded: true,
        isLoading: false,
        error: null,
        lastLoadTime: null,
      });

      searchStateSignal.set({
        searchText: 'test',
        filterType: null,
        results: [mockFileItem1, mockFileItem2],
        isSearching: false,
        hasSearched: true,
        error: null,
      });
    });

    it('should highlight currently playing file in search mode', () => {
      currentFileSignal.set(
        createLaunchedFile(mockFileItem1, '/test', 'test-device', StorageType.Usb)
      );
      launchModeSignal.set(LaunchMode.Search);

      fixture.detectChanges();

      expect(component.isCurrentlyPlaying(mockFileItem1)).toBe(true);
      expect(component.isCurrentlyPlaying(mockFileItem2)).toBe(false);
    });

    it('should NOT highlight playing file in directory mode', () => {
      currentFileSignal.set(
        createLaunchedFile(mockFileItem1, '/test', 'test-device', StorageType.Usb)
      );
      launchModeSignal.set(LaunchMode.Directory);

      fixture.detectChanges();

      expect(component.isCurrentlyPlaying(mockFileItem1)).toBe(false);
    });

    it('should show error state for playing file with error', () => {
      currentFileSignal.set(
        createLaunchedFile(mockFileItem1, '/test', 'test-device', StorageType.Usb)
      );
      launchModeSignal.set(LaunchMode.Search);
      errorSignal.set('Playback failed');

      fixture.detectChanges();

      expect(component.hasPlayerError()).toBe(true);
    });

    it('should auto-select playing file on playback start', () => {
      launchModeSignal.set(LaunchMode.Search);
      fixture.detectChanges();

      // Initially no file selected
      expect(component.selectedItem()).toBeNull();

      // Start playing a file
      currentFileSignal.set(
        createLaunchedFile(mockFileItem1, '/test', 'test-device', StorageType.Usb)
      );
      fixture.detectChanges();

      // File should be auto-selected
      expect(component.selectedItem()).toEqual(mockFileItem1);
    });
  });

  describe('Edge Cases and Error Handling', () => {
    it('should handle null search state gracefully', () => {
      searchStateSignal.set(null);
      fixture.detectChanges();

      expect(() => fixture.detectChanges()).not.toThrow();
      expect(component.searchResults()).toEqual([]);
      expect(component.isSearching()).toBe(false);
      expect(component.hasSearched()).toBe(false);
      expect(component.searchError()).toBeNull();
    });

    it('should handle empty deviceId without crashing', () => {
      fixture.componentRef.setInput('deviceId', '');
      fixture.detectChanges();

      expect(() => fixture.detectChanges()).not.toThrow();
    });

    it('should handle missing file in search results during playback', () => {
      const differentFile: FileItem = {
        path: '/other/file3.sid',
        name: 'file3.sid',
        size: 3072,
        type: FileItemType.Song,
        images: [],
        parentPath: '/other',
        description: 'Different file',
        isFavorite: false,
        isCompatible: true,
        title: '',
        creator: '',
        releaseInfo: '',
        shareUrl: '',
        metadataSource: '',
        meta1: '',
        meta2: '',
        metadataSourcePath: '',
        playLength: '',
        subtuneLengths: [],
        startSubtuneNum: 0,
      };

      searchStateSignal.set({
        searchText: 'test',
        filterType: null,
        results: [mockFileItem1, mockFileItem2],
        isSearching: false,
        hasSearched: true,
        error: null,
      });

      currentFileSignal.set(
        createLaunchedFile(differentFile, '/other', 'test-device', StorageType.Usb)
      );
      launchModeSignal.set(LaunchMode.Search);

      fixture.detectChanges();

      // Component should not crash, just not highlight any file
      expect(() => fixture.detectChanges()).not.toThrow();
      expect(component.isCurrentlyPlaying(mockFileItem1)).toBe(false);
      expect(component.isCurrentlyPlaying(mockFileItem2)).toBe(false);
    });

    it('should not crash when double-clicking without search state', async () => {
      searchStateSignal.set(null);
      fixture.detectChanges();

      // Should not throw
      await expect(component.onFileDoubleClick(mockFileItem1)).resolves.toBeUndefined();

      // Should not have called launchFileWithContext
      expect(mockPlayerContext.launchFileWithContext).not.toHaveBeenCalled();
    });
  });
});

// Helper function to create LaunchedFile objects
function createLaunchedFile(
  file: FileItem,
  parentPath: string,
  deviceId: string,
  storageType: StorageType
): LaunchedFile {
  return {
    storageKey: StorageKeyUtil.create(deviceId, storageType),
    file,
    parentPath,
    launchedAt: Date.now(),
    launchMode: LaunchMode.Search,
    isCompatible: true,
  };
}
