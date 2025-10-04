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
  let mockStorageStore: Partial<StorageStore>;
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
  };

  const mockStorageService: Partial<IStorageService> = {    getDirectory: () => of({} as StorageDirectory),
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
    const combined = component.combinedItems();
    expect(combined).toHaveLength(2);
    expect(combined[0]).toHaveProperty('itemType', 'directory');
    expect(combined[1]).toHaveProperty('itemType', 'file');
  });

  it('should correctly identify directories with type guard', () => {
    const combined = component.combinedItems();
    expect(component.isDirectory(combined[0])).toBe(true);
    expect(component.isDirectory(combined[1])).toBe(false);
  });

  it('should update selection when item clicked', () => {
    const combined = component.combinedItems();
    component.onItemSelected(combined[0]);
    expect(component.selectedItem()).toEqual(combined[0]);
  });

  it('should call navigateToDirectory on directory double-click', () => {
    const combined = component.combinedItems();
    const directoryItem = combined[0] as DirectoryItem;

    component.onDirectoryDoubleClick(directoryItem);

    expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
      deviceId: 'device-1',
      storageType: StorageType.Sd,
      path: directoryItem.path,
    });
  });

  it('should clear selection when directory changes', () => {
    const combined = component.combinedItems();
    component.onItemSelected(combined[0]);
    expect(component.selectedItem()).toBeTruthy();

    component.onDirectoryDoubleClick(combined[0] as DirectoryItem);
    expect(component.selectedItem()).toBe(null);
  });

  it('should call player context on file double-click', () => {
    const combined = component.combinedItems();
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
    const fileElement: HTMLElement | null = fixture.nativeElement.querySelector(
      'lib-file-item .file-item'
    );
    expect(fileElement).toBeTruthy();

    fileElement?.dispatchEvent(new MouseEvent('dblclick'));

    expect(mockPlayerContext.launchFileWithContext).toHaveBeenCalled();
  });

  it('should render correct number of list items', () => {
    const items = fixture.nativeElement.querySelectorAll('.file-list-item');
    expect(items.length).toBe(2);
  });

  it('should determine if item is selected correctly', () => {
    const combined = component.combinedItems();
    component.onItemSelected(combined[0]);

    expect(component.isSelected(combined[0])).toBe(true);
    expect(component.isSelected(combined[1])).toBe(false);
  });

  it('should identify currently playing file', () => {
    const combined = component.combinedItems();
    const fileItem = combined[1] as FileItem;
    
    // Initially no file is playing
    expect(component.isCurrentlyPlaying(fileItem)).toBe(false);
    
    // Get the signal and update it
    (mockPlayerContext.getCurrentFile as vi.MockedFunction<() => unknown>).mockReturnValue(
      signal({
        deviceId: 'device-1',
        storageType: StorageType.Sd,
        file: fileItem,
        isShuffleMode: false
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
    const combined = component.combinedItems();
    const fileItem = combined[1] as FileItem;
    
    // Mock both signals with the playing file and context
    (mockPlayerContext.getCurrentFile as vi.MockedFunction<() => unknown>).mockReturnValue(
      signal({
        deviceId: 'device-1',
        storageType: StorageType.Sd,
        file: fileItem,
        isShuffleMode: false
      }).asReadonly()
    );
    
    (mockPlayerContext.getFileContext as vi.MockedFunction<() => unknown>).mockReturnValue(
      signal({
        directoryPath: '/test',
        files: [mockFileItem],
        currentIndex: 0
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
      (mockPlayerContext.getError as vi.MockedFunction<() => unknown>).mockReturnValue(
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
      const combined = component.combinedItems();
      const fileItem = combined[1] as FileItem;

      // Set file as currently playing
      (mockPlayerContext.getCurrentFile as vi.MockedFunction<() => unknown>).mockReturnValue(
        signal({
          deviceId: 'device-1',
          storageType: StorageType.Sd,
          file: fileItem,
          isShuffleMode: false
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
      const combined = component.combinedItems();
      const fileItem = combined[1] as FileItem;

      // Set file as currently playing with error
      const errorSignal = signal<string | null>('Launch failed: Incompatible file format');
      (mockPlayerContext.getCurrentFile as vi.MockedFunction<() => unknown>).mockReturnValue(
        signal({
          deviceId: 'device-1',
          storageType: StorageType.Sd,
          file: fileItem,
          isShuffleMode: false
        }).asReadonly()
      );
      (mockPlayerContext.getError as vi.MockedFunction<() => unknown>).mockReturnValue(
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
});
