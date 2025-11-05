import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { vi } from 'vitest';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { DirectoryFilesComponent } from './directory-files.component';
import {
  DirectoryItem,
  FileItem,
  FileItemType,
  STORAGE_SERVICE,
  IStorageService,
  StorageDirectory,
  StorageType,
  LaunchMode,
  PlayerStatus,
} from '@teensyrom-nx/domain';
import {
  StorageStore,
  PLAYER_CONTEXT,
  IPlayerContext,
  LaunchedFile,
  PlayerFileContext,
} from '@teensyrom-nx/application';

describe('DirectoryFilesComponent', () => {
  let component: DirectoryFilesComponent;
  let fixture: ComponentFixture<DirectoryFilesComponent>;
  let mockStorageStore: Partial<typeof StorageStore>;
  let mockPlayerContext: Partial<IPlayerContext>;

  const mockDirectoryItem: DirectoryItem = {
    name: 'Test Folder',
    path: '/test/folder',
  };

  const mockFileItem: FileItem = {
    name: 'test-file.sid',
    path: '/test/file.sid',
    size: 1024,
    type: FileItemType.Song,
    isFavorite: false,
    title: '',
    creator: '',
    releaseInfo: '',
    description: '',
    shareUrl: '',
    metadataSource: '',
    meta1: '',
    meta2: '',
    metadataSourcePath: '',
    parentPath: '/test',
    playLength: '',
    subtuneLengths: [],
    startSubtuneNum: 0,
    images: [],
    isCompatible: true,
  };

  const mockStorageService: Partial<IStorageService> = {
    getDirectory: () => of({} as StorageDirectory),
  };

  beforeEach(async () => {
    const playerCurrentFileSignal = signal<LaunchedFile | null>(null);
    const playerContextSignal = signal<PlayerFileContext | null>(null);
    const loadingSignal = signal(false);
    const errorSignal = signal<string | null>(null);
    const statusSignal = signal(PlayerStatus.Stopped);

    mockStorageStore = {
      getSelectedDirectoryState: vi.fn().mockReturnValue(
        signal({
          directory: {
            directories: [mockDirectoryItem],
            files: [mockFileItem],
            path: '/test',
          },
          currentPath: '/test',
          storageType: StorageType.Sd,
          deviceId: 'device-1',
          isLoading: false,
          isLoaded: true,
          error: null,
        })
      ),
      navigateToDirectory: vi.fn(),
    };

    mockPlayerContext = {
      initializePlayer: vi.fn(),
      removePlayer: vi.fn(),
      launchFileWithContext: vi.fn().mockResolvedValue(undefined),
      getCurrentFile: vi.fn().mockReturnValue(playerCurrentFileSignal.asReadonly()),
      getFileContext: vi.fn().mockReturnValue(playerContextSignal.asReadonly()),
      isLoading: vi.fn().mockReturnValue(loadingSignal.asReadonly()),
      getError: vi.fn().mockReturnValue(errorSignal.asReadonly()),
      getStatus: vi.fn().mockReturnValue(statusSignal.asReadonly()),
      getLaunchMode: vi.fn().mockReturnValue(signal(LaunchMode.Directory).asReadonly()),
    };

    await TestBed.configureTestingModule({
      imports: [DirectoryFilesComponent],
      providers: [
        provideNoopAnimations(),
        { provide: STORAGE_SERVICE, useValue: mockStorageService },
        { provide: StorageStore, useValue: mockStorageStore },
        { provide: PLAYER_CONTEXT, useValue: mockPlayerContext },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectoryFilesComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'device-1');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should combine directories and files into single data source', () => {
    const combined = component.directoriesAndFiles();
    expect(combined).toHaveLength(2);
    expect(combined[0]).toHaveProperty('itemType', 'directory');
    expect(combined[1]).toHaveProperty('itemType', 'file');
  });

  it('should correctly identify directories with type guard', () => {
    const combined = component.directoriesAndFiles();
    expect(component.isDirectory(combined[0])).toBe(true);
    expect(component.isDirectory(combined[1])).toBe(false);
  });

  it('should update selection when item clicked', () => {
    const combined = component.directoriesAndFiles();
    component.onItemSelected(combined[0]);
    expect(component.selectedItem()).toEqual(combined[0]);
  });

  it('should call navigateToDirectory on directory double-click', () => {
    const combined = component.directoriesAndFiles();
    const directoryItem = combined[0] as DirectoryItem;

    component.onDirectoryDoubleClick(directoryItem);

    expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
      deviceId: 'device-1',
      storageType: StorageType.Sd,
      path: directoryItem.path,
    });
  });

  it('should clear selection when directory changes', () => {
    const combined = component.directoriesAndFiles();
    component.onItemSelected(combined[0]);
    expect(component.selectedItem()).toBeTruthy();

    component.onDirectoryDoubleClick(combined[0] as DirectoryItem);
    expect(component.selectedItem()).toBe(null);
  });

  it('should call player context on file double-click', () => {
    const combined = component.directoriesAndFiles();
    const fileItem = combined[1] as FileItem;

    component.onFileDoubleClick(fileItem);

    expect(mockPlayerContext.launchFileWithContext).toHaveBeenCalledWith({
      deviceId: 'device-1',
      storageType: StorageType.Sd,
      file: fileItem,
      directoryPath: '/test',
      files: expect.arrayContaining([mockFileItem]),
      launchMode: LaunchMode.Directory,
    });
  });

  it('should trigger player context when file is double-clicked via template', () => {
    // Wait for viewport to render items
    fixture.detectChanges();

    const fileElement: HTMLElement | null = fixture.nativeElement.querySelector('lib-file-item');
    expect(fileElement).toBeTruthy();

    // Trigger the double-click on the lib-storage-item which emits the activated event
    const storageItemElement = fileElement?.querySelector('lib-storage-item');
    expect(storageItemElement).toBeTruthy();

    storageItemElement?.dispatchEvent(new MouseEvent('dblclick'));
    fixture.detectChanges();

    expect(mockPlayerContext.launchFileWithContext).toHaveBeenCalled();
  });

  it('should render items in virtual scroll viewport', () => {
    // Virtual scrolling may not render all items at once, only visible + buffer
    const viewport = fixture.nativeElement.querySelector('cdk-virtual-scroll-viewport');
    expect(viewport).toBeTruthy();

    const items = fixture.nativeElement.querySelectorAll('.file-list-item');
    // Should render at least some items (depends on viewport size and buffer)
    expect(items.length).toBeGreaterThan(0);
    expect(items.length).toBeLessThanOrEqual(2); // With only 2 total items
  });

  it('should determine if item is selected correctly', () => {
    const combined = component.directoriesAndFiles();
    component.onItemSelected(combined[0]);

    expect(component.isSelected(combined[0])).toBe(true);
    expect(component.isSelected(combined[1])).toBe(false);
  });

  it('should identify currently playing file', () => {
    const combined = component.directoriesAndFiles();
    const fileItem = combined[1] as FileItem;

    // Initially no file is playing
    expect(component.isCurrentlyPlaying(fileItem)).toBe(false);

    // Get the signal and update it
    (mockPlayerContext.getCurrentFile as ReturnType<typeof vi.fn>).mockReturnValue(
      signal({
        deviceId: 'device-1',
        storageType: StorageType.Sd,
        file: fileItem,
        isShuffleMode: false,
      }).asReadonly()
    );

    // Re-create component to pick up the new signal
    fixture = TestBed.createComponent(DirectoryFilesComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'device-1');
    fixture.detectChanges();

    expect(component.isCurrentlyPlaying(fileItem)).toBe(true);
  });

  it('should auto-select currently playing file when directory context is available', () => {
    const combined = component.directoriesAndFiles();
    const fileItem = combined[1] as FileItem;

    // Mock both signals with the playing file and context
    (mockPlayerContext.getCurrentFile as ReturnType<typeof vi.fn>).mockReturnValue(
      signal({
        deviceId: 'device-1',
        storageType: StorageType.Sd,
        file: fileItem,
        isShuffleMode: false,
      }).asReadonly()
    );

    (mockPlayerContext.getFileContext as ReturnType<typeof vi.fn>).mockReturnValue(
      signal({
        directoryPath: '/test',
        files: [mockFileItem],
        currentIndex: 0,
      }).asReadonly()
    );

    // Re-create component to pick up the new signals
    fixture = TestBed.createComponent(DirectoryFilesComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'device-1');
    fixture.detectChanges();

    // The file should be auto-selected
    expect(component.selectedItem()?.path).toBe(fileItem.path);
  });

  describe('Highlight Behavior', () => {
    it('should return false from hasCurrentFileError when no error exists', () => {
      // Initially no error
      expect(component.hasCurrentFileError()).toBe(false);
    });

    it('should return true from hasCurrentFileError when error exists', () => {
      const errorSignal = signal<string | null>('Launch failed: Incompatible file format');
      (mockPlayerContext.getError as ReturnType<typeof vi.fn>).mockReturnValue(
        errorSignal.asReadonly()
      );

      // Re-create component to pick up the new signal
      fixture = TestBed.createComponent(DirectoryFilesComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'device-1');
      fixture.detectChanges();

      expect(component.hasCurrentFileError()).toBe(true);
    });

    it('should render data-is-playing attribute for currently playing file', () => {
      const combined = component.directoriesAndFiles();
      const fileItem = combined[1] as FileItem;

      // Set file as currently playing
      (mockPlayerContext.getCurrentFile as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          deviceId: 'device-1',
          storageType: StorageType.Sd,
          file: fileItem,
          isShuffleMode: false,
        }).asReadonly()
      );

      // Re-create component to pick up the new signal
      fixture = TestBed.createComponent(DirectoryFilesComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'device-1');
      fixture.detectChanges();

      // Find the element with the file path
      const fileElement: HTMLElement | null = fixture.nativeElement.querySelector(
        `.file-list-item[data-item-path="${fileItem.path}"]`
      );
      expect(fileElement).toBeTruthy();
      expect(fileElement?.getAttribute('data-is-playing')).toBe('true');
    });

    it('should render data-has-error attribute when error exists', () => {
      const combined = component.directoriesAndFiles();
      const fileItem = combined[1] as FileItem;

      // Set file as currently playing with error
      const errorSignal = signal<string | null>('Launch failed: Incompatible file format');
      (mockPlayerContext.getCurrentFile as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          deviceId: 'device-1',
          storageType: StorageType.Sd,
          file: fileItem,
          isShuffleMode: false,
        }).asReadonly()
      );
      (mockPlayerContext.getError as ReturnType<typeof vi.fn>).mockReturnValue(
        errorSignal.asReadonly()
      );

      // Re-create component to pick up the new signals
      fixture = TestBed.createComponent(DirectoryFilesComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'device-1');
      fixture.detectChanges();

      // Find the element with the file path
      const fileElement: HTMLElement | null = fixture.nativeElement.querySelector(
        `.file-list-item[data-item-path="${fileItem.path}"]`
      );
      expect(fileElement).toBeTruthy();
      expect(fileElement?.getAttribute('data-is-playing')).toBe('true');
      expect(fileElement?.getAttribute('data-has-error')).toBe('true');
    });
  });

  describe('Virtual Scrolling', () => {
    it('should initialize viewport with correct configuration', () => {
      const viewport = fixture.nativeElement.querySelector('cdk-virtual-scroll-viewport');
      expect(viewport).toBeTruthy();
      expect(viewport.getAttribute('itemsize')).toBe('52');
    });

    it('should automatically scroll to playing file', async () => {
      const combined = component.directoriesAndFiles();
      const fileItem = combined[1] as FileItem;

      // Mock getCurrentFile and fileContext to trigger auto-scroll
      (mockPlayerContext.getCurrentFile as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          deviceId: 'device-1',
          storageType: StorageType.Sd,
          file: fileItem,
          isShuffleMode: false,
        }).asReadonly()
      );

      (mockPlayerContext.getFileContext as ReturnType<typeof vi.fn>).mockReturnValue(
        signal({
          directoryPath: '/test',
          files: [mockFileItem],
          currentIndex: 0,
        }).asReadonly()
      );

      // Re-create component to pick up the new signals and trigger the effect
      fixture = TestBed.createComponent(DirectoryFilesComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'device-1');
      fixture.detectChanges();

      // Wait for effects and animations
      await new Promise((resolve) => setTimeout(resolve, 100));

      // Verify the playing file is visible in the viewport
      const fileElement = fixture.nativeElement.querySelector(
        `.file-list-item[data-item-path="${fileItem.path}"]`
      );
      expect(fileElement).toBeTruthy();
    });

    // Note: Testing with large datasets requires isolated TestBed configuration
    // which is difficult in the current test structure. Virtual scrolling behavior
    // is validated through manual testing and the integration tests above confirm
    // the viewport is properly configured and functional.
  });
});
