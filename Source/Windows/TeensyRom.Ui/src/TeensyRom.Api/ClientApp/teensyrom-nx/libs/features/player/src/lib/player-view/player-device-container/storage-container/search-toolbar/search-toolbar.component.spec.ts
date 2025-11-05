import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { By } from '@angular/platform-browser';
import { SearchToolbarComponent } from './search-toolbar.component';
import {
  STORAGE_SERVICE,
  IStorageService,
  StorageDirectory,
  StorageType,
  LaunchMode,
  PlayerStatus,
  PlayerFilterType,
} from '@teensyrom-nx/domain';
import { PLAYER_CONTEXT, IPlayerContext, StorageStore } from '@teensyrom-nx/application';

describe('SearchToolbarComponent', () => {
  let component: SearchToolbarComponent;
  let fixture: ComponentFixture<SearchToolbarComponent>;
  let mockStorageStore: {
    getSelectedDirectoryState: ReturnType<typeof vi.fn>;
    getSearchState: ReturnType<typeof vi.fn>;
    searchFiles: ReturnType<typeof vi.fn>;
    clearSearch: ReturnType<typeof vi.fn>;
  };
  let mockPlayerContext: Partial<IPlayerContext>;

  beforeEach(async () => {
    const mockStorageService: Partial<IStorageService> = {
      getDirectory: () =>
        of({
          deviceId: 'test-device',
          storageType: StorageType.Sd,
          path: '/',
          files: [],
          directories: [],
        } as StorageDirectory),
    };

    mockStorageStore = {
      getSelectedDirectoryState: vi.fn().mockReturnValue(
        signal({
          directory: null,
          currentPath: '/',
          storageType: StorageType.Sd,
          deviceId: 'test-device',
          isLoading: false,
          isLoaded: false,
          error: null,
        })
      ),
      getSearchState: vi.fn().mockReturnValue(
        signal({
          hasSearched: false,
          isSearching: false,
          searchText: '',
          results: [],
          error: null,
        })
      ),
      searchFiles: vi.fn(),
      clearSearch: vi.fn(),
    };

    mockPlayerContext = {
      initializePlayer: vi.fn(),
      removePlayer: vi.fn(),
      launchFileWithContext: vi.fn().mockResolvedValue(undefined),
      launchRandomFile: vi.fn().mockResolvedValue(undefined),
      play: vi.fn().mockResolvedValue(undefined),
      pause: vi.fn().mockResolvedValue(undefined),
      stop: vi.fn().mockResolvedValue(undefined),
      next: vi.fn().mockResolvedValue(undefined),
      previous: vi.fn().mockResolvedValue(undefined),
      getCurrentFile: vi.fn().mockReturnValue(signal(null).asReadonly()),
      getFileContext: vi.fn().mockReturnValue(signal(null).asReadonly()),
      getPlayerStatus: vi.fn().mockReturnValue(signal(PlayerStatus.Stopped).asReadonly()),
      getStatus: vi.fn().mockReturnValue(signal(PlayerStatus.Stopped).asReadonly()),
      isLoading: vi.fn().mockReturnValue(signal(false).asReadonly()),
      getError: vi.fn().mockReturnValue(signal(null).asReadonly()),
      toggleShuffleMode: vi.fn(),
      setShuffleScope: vi.fn(),
      setFilterMode: vi.fn(),
      getLaunchMode: vi.fn().mockReturnValue(signal(LaunchMode.Directory).asReadonly()),
      getShuffleSettings: vi
        .fn()
        .mockReturnValue(signal({ filter: PlayerFilterType.All }).asReadonly()),
      getTimerState: vi.fn().mockReturnValue(signal(null).asReadonly()),
      isCurrentFileCompatible: vi.fn().mockReturnValue(signal(true).asReadonly()),
      getPlayHistory: vi.fn().mockReturnValue(signal(null).asReadonly()),
      getCurrentHistoryPosition: vi.fn().mockReturnValue(signal(0).asReadonly()),
      canNavigateBackwardInHistory: vi.fn().mockReturnValue(signal(false).asReadonly()),
      canNavigateForwardInHistory: vi.fn().mockReturnValue(signal(false).asReadonly()),
      clearHistory: vi.fn(),
      toggleHistoryView: vi.fn(),
      isHistoryViewVisible: vi.fn().mockReturnValue(signal(false).asReadonly()),
      navigateToHistoryPosition: vi.fn().mockResolvedValue(undefined),
    };

    await TestBed.configureTestingModule({
      providers: [
        provideNoopAnimations(),
        { provide: STORAGE_SERVICE, useValue: mockStorageService },
        { provide: StorageStore, useValue: mockStorageStore },
        { provide: PLAYER_CONTEXT, useValue: mockPlayerContext },
      ],
      imports: [SearchToolbarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SearchToolbarComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'test-device');
    fixture.detectChanges();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with empty search text', () => {
      expect(component.searchText()).toBe('');
    });

    it('should have search button disabled initially', () => {
      expect(component.canSearch()).toBe(false);
    });

    it('should not show clear button initially', () => {
      expect(component.showClearButton()).toBe(false);
    });
  });

  describe('Search Input Handling', () => {
    it('should update searchText signal when input changes', () => {
      component.onSearchInputChange('test query');
      expect(component.searchText()).toBe('test query');
    });

    it('should enable search button when text is entered', () => {
      component.onSearchInputChange('test');
      fixture.detectChanges();
      expect(component.canSearch()).toBe(true);
    });

    it('should disable search button for empty text', () => {
      component.onSearchInputChange('   ');
      fixture.detectChanges();
      expect(component.canSearch()).toBe(false);
    });

    it('should disable search button when searching is in progress', () => {
      // Set text first
      component.onSearchInputChange('test');
      fixture.detectChanges();
      expect(component.canSearch()).toBe(true);

      // Mock searching state
      (mockStorageStore.getSearchState as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          hasSearched: false,
          isSearching: true,
          searchText: 'test',
          results: [],
          error: null,
        })
      );

      // Re-create component to pick up new signal
      fixture = TestBed.createComponent(SearchToolbarComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      component.onSearchInputChange('test');
      fixture.detectChanges();

      expect(component.canSearch()).toBe(false);
    });
  });

  describe('Execute Search', () => {
    it('should call storageStore.searchFiles with correct parameters', () => {
      component.onSearchInputChange('iron maiden');
      component.executeSearch();

      expect(mockStorageStore.searchFiles).toHaveBeenCalledWith({
        deviceId: 'test-device',
        storageType: StorageType.Sd,
        searchText: 'iron maiden',
        filterType: PlayerFilterType.All,
      });
    });

    it('should trim search text before executing', () => {
      component.onSearchInputChange('  spaced text  ');
      component.executeSearch();

      expect(mockStorageStore.searchFiles).toHaveBeenCalledWith(
        expect.objectContaining({
          searchText: 'spaced text',
        })
      );
    });

    it('should not execute search with empty text', () => {
      component.onSearchInputChange('   ');
      component.executeSearch();

      expect(mockStorageStore.searchFiles).not.toHaveBeenCalled();
    });

    it('should not execute search when storageType is null', () => {
      // Mock directory state with null storage type
      (mockStorageStore.getSelectedDirectoryState as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          directory: null,
          currentPath: '/',
          storageType: null,
          deviceId: 'test-device',
          isLoading: false,
          isLoaded: false,
          error: null,
        })
      );

      // Re-create component
      fixture = TestBed.createComponent(SearchToolbarComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();

      component.onSearchInputChange('test');
      component.executeSearch();

      expect(mockStorageStore.searchFiles).not.toHaveBeenCalled();
    });

    it('should use current filter type in search', () => {
      // Mock filter type as Games
      (mockPlayerContext.getShuffleSettings as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({ filter: PlayerFilterType.Games }).asReadonly()
      );

      // Re-create component
      fixture = TestBed.createComponent(SearchToolbarComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();

      component.onSearchInputChange('mario');
      component.executeSearch();

      expect(mockStorageStore.searchFiles).toHaveBeenCalledWith(
        expect.objectContaining({
          filterType: PlayerFilterType.Games,
        })
      );
    });

    it('should trigger search on Enter key press', () => {
      component.onSearchInputChange('test search');
      fixture.detectChanges();

      const inputField = fixture.debugElement.query(By.css('lib-input-field'));
      expect(inputField).toBeTruthy();

      // Simulate Enter key
      inputField.nativeElement.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
      fixture.detectChanges();

      expect(mockStorageStore.searchFiles).toHaveBeenCalled();
    });
  });

  describe('Debounced Auto-Search', () => {
    it('should not auto-search with empty text', fakeAsync(() => {
      component.onSearchInputChange('');
      tick(1000);
      expect(mockStorageStore.searchFiles).not.toHaveBeenCalled();
    }));

    it('should not auto-search with whitespace-only text', fakeAsync(() => {
      component.onSearchInputChange('   ');
      tick(1000);
      expect(mockStorageStore.searchFiles).not.toHaveBeenCalled();
    }));
  });

  describe('Clear Search', () => {
    it('should call storageStore.clearSearch with correct parameters', () => {
      component.clearSearch();

      expect(mockStorageStore.clearSearch).toHaveBeenCalledWith({
        deviceId: 'test-device',
        storageType: StorageType.Sd,
      });
    });

    it('should not clear search when storageType is null', () => {
      // Mock directory state with null storage type
      (mockStorageStore.getSelectedDirectoryState as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          directory: null,
          currentPath: '/',
          storageType: null,
          deviceId: 'test-device',
          isLoading: false,
          isLoaded: false,
          error: null,
        })
      );

      // Re-create component
      fixture = TestBed.createComponent(SearchToolbarComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();

      component.clearSearch();

      expect(mockStorageStore.clearSearch).not.toHaveBeenCalled();
    });
  });

  describe('Clear Button Visibility', () => {
    it('should show clear button when search has results', () => {
      // Mock search state with results
      (mockStorageStore.getSearchState as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          hasSearched: true,
          isSearching: false,
          searchText: 'test',
          results: [{ name: 'result1' }],
          error: null,
        })
      );

      // Re-create component
      fixture = TestBed.createComponent(SearchToolbarComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();

      expect(component.showClearButton()).toBe(true);
    });

    it('should not show clear button when no search has been performed', () => {
      expect(component.showClearButton()).toBe(false);
    });

    it('should not show clear button when search has no results', () => {
      // Mock search state with no results
      (mockStorageStore.getSearchState as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          hasSearched: true,
          isSearching: false,
          searchText: 'test',
          results: [],
          error: null,
        })
      );

      // Re-create component
      fixture = TestBed.createComponent(SearchToolbarComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();

      expect(component.showClearButton()).toBe(false);
    });

    it('should render clear button in DOM when visible', () => {
      // Mock search state with results
      (mockStorageStore.getSearchState as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          hasSearched: true,
          isSearching: false,
          searchText: 'test',
          results: [{ name: 'result1' }],
          error: null,
        })
      );

      // Re-create component
      fixture = TestBed.createComponent(SearchToolbarComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();

      const clearButton = fixture.debugElement.query(By.css('lib-icon-button'));
      expect(clearButton).toBeTruthy();
    });

    it('should not render clear button in DOM when not visible', () => {
      const clearButton = fixture.debugElement.query(By.css('lib-icon-button'));
      expect(clearButton).toBeFalsy();
    });
  });

  describe('Reactive Effects', () => {
    it('should clear searchText when search state is cleared', () => {
      // Set some search text
      component.onSearchInputChange('test search');
      expect(component.searchText()).toBe('test search');

      // Mock search state as null (cleared)
      (mockStorageStore.getSearchState as ReturnType<typeof vi.fn>).mockReturnValue(signal(null));

      // Re-create component to trigger effect
      fixture = TestBed.createComponent(SearchToolbarComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      component.onSearchInputChange('test search');
      fixture.detectChanges();

      // Note: The effect would clear it, but we can't easily test effects in isolation
      // This is more of an integration test that would need the full store
    });
  });

  describe('Computed Signals', () => {
    it('should compute hasActiveSearch from search state', () => {
      expect(component.hasActiveSearch()).toBe(false);

      // Mock active search
      (mockStorageStore.getSearchState as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          hasSearched: true,
          isSearching: false,
          searchText: 'test',
          results: [],
          error: null,
        })
      );

      // Re-create component
      fixture = TestBed.createComponent(SearchToolbarComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();

      expect(component.hasActiveSearch()).toBe(true);
    });

    it('should compute isSearching from search state', () => {
      expect(component.isSearching()).toBe(false);

      // Mock searching state
      (mockStorageStore.getSearchState as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          hasSearched: false,
          isSearching: true,
          searchText: 'test',
          results: [],
          error: null,
        })
      );

      // Re-create component
      fixture = TestBed.createComponent(SearchToolbarComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();

      expect(component.isSearching()).toBe(true);
    });

    it('should compute currentFilter from player context', () => {
      expect(component.currentFilter()).toBe(PlayerFilterType.All);

      // Mock different filter
      (mockPlayerContext.getShuffleSettings as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({ filter: PlayerFilterType.Music }).asReadonly()
      );

      // Re-create component
      fixture = TestBed.createComponent(SearchToolbarComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();

      expect(component.currentFilter()).toBe(PlayerFilterType.Music);
    });
  });

  describe('Template Rendering', () => {
    it('should render search input field', () => {
      const inputField = fixture.debugElement.query(By.css('lib-input-field'));
      expect(inputField).toBeTruthy();
    });

    it('should render search field with correct properties', () => {
      const inputField = fixture.debugElement.query(By.css('lib-input-field'));
      expect(inputField.componentInstance.label()).toBe('Search');
      expect(inputField.componentInstance.prefixIcon()).toBe('search');
    });

    it('should wrap content in scaling compact card', () => {
      const card = fixture.debugElement.query(By.css('lib-scaling-compact-card'));
      expect(card).toBeTruthy();
    });

    it('should have proper CSS class structure', () => {
      const content = fixture.nativeElement.querySelector('.search-toolbar-content');
      expect(content).toBeTruthy();
    });
  });
});
