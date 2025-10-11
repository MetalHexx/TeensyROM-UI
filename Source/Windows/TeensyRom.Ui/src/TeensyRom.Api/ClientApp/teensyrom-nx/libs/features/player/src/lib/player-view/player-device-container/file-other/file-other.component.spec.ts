import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { signal } from '@angular/core';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { By } from '@angular/platform-browser';
import { FileOtherComponent } from './file-other.component';
import { PLAYER_CONTEXT, IPlayerContext } from '@teensyrom-nx/application';
import { LaunchMode, PlayerStatus, FileItemType, StorageType } from '@teensyrom-nx/domain';

describe('FileOtherComponent', () => {
  let component: FileOtherComponent;
  let fixture: ComponentFixture<FileOtherComponent>;
  let mockPlayerContext: Partial<IPlayerContext>;
  let currentFileSignal: ReturnType<typeof signal>;

  beforeEach(async () => {
    // Create writable signal for testing
    currentFileSignal = signal(null);

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
      getCurrentFile: vi.fn().mockReturnValue(currentFileSignal.asReadonly()),
      getFileContext: vi.fn().mockReturnValue(signal(null).asReadonly()),
      getPlayerStatus: vi.fn().mockReturnValue(signal(PlayerStatus.Stopped).asReadonly()),
      getStatus: vi.fn().mockReturnValue(signal(PlayerStatus.Stopped).asReadonly()),
      isLoading: vi.fn().mockReturnValue(signal(false).asReadonly()),
      getError: vi.fn().mockReturnValue(signal(null).asReadonly()),
      toggleShuffleMode: vi.fn(),
      setShuffleScope: vi.fn(),
      setFilterMode: vi.fn(),
      getLaunchMode: vi.fn().mockReturnValue(signal(LaunchMode.Directory).asReadonly()),
      getShuffleSettings: vi.fn().mockReturnValue(signal(null).asReadonly()),
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
        { provide: PLAYER_CONTEXT, useValue: mockPlayerContext },
      ],
      imports: [FileOtherComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FileOtherComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'test-device');
    fixture.detectChanges();
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should require deviceId input', () => {
      expect(component.deviceId()).toBe('test-device');
    });
  });

  describe('Empty State', () => {
    it('should show empty state when no file is loaded', () => {
      currentFileSignal.set(null);
      fixture.detectChanges();

      const emptyState = fixture.debugElement.query(By.css('lib-empty-state-message'));
      expect(emptyState).toBeTruthy();
      expect(component.hasFile()).toBe(false);
    });

    it('should display "No File Launched" message in empty state', () => {
      currentFileSignal.set(null);
      fixture.detectChanges();

      const emptyState = fixture.debugElement.query(By.css('lib-empty-state-message'));
      expect(emptyState).toBeTruthy();
      
      // Verify the rendered text content
      const emptyStateElement = emptyState.nativeElement as HTMLElement;
      const textContent = emptyStateElement.textContent || '';
      expect(textContent).toContain('No File Launched');
      expect(textContent).toContain('Launch a file from the file browser below.');
    });

    it('should show secondary message about dice button', () => {
      currentFileSignal.set(null);
      fixture.detectChanges();

      const emptyState = fixture.debugElement.query(By.css('lib-empty-state-message'));
      const emptyStateElement = emptyState.nativeElement as HTMLElement;
      const textContent = emptyStateElement.textContent || '';
      expect(textContent).toContain('Try clicking the dice button to launch a random file.');
    });
  });

  describe('File Metadata Display', () => {
    it('should display file with title and description', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.prg',
          path: '/test/test.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Game',
          creator: '',
          releaseInfo: '2024',
          description: 'A test game description',
          shareUrl: '',
          metadataSource: 'GameBase64',
          meta1: 'PRG',
          meta2: 'C64',
          metadataSourcePath: '',
          parentPath: '/test',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.title()).toBe('Test Game');
      expect(component.description()).toBe('A test game description');
      expect(component.hasContent()).toBe(true);
      expect(component.hasFile()).toBe(true);
    });

    it('should display filename when title is empty', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.prg',
          path: '/test/test.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: '',
          creator: '',
          releaseInfo: '',
          description: 'Game description',
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
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.filename()).toBe('test.prg');
      expect(component.title()).toBe('');
    });

    it('should display release info as subtitle', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'game.prg',
          path: '/games/game.prg',
          type: FileItemType.Game,
          size: 2048,
          isFavorite: false,
          isCompatible: true,
          title: 'Classic Game',
          creator: 'Developer',
          releaseInfo: '1985',
          description: '',
          shareUrl: '',
          metadataSource: 'GameBase64',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/games',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.releaseInfo()).toBe('1985');
    });

    it('should display metadata source', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'game.prg',
          path: '/games/game.prg',
          type: FileItemType.Game,
          size: 2048,
          isFavorite: false,
          isCompatible: true,
          title: 'Game',
          creator: '',
          releaseInfo: '',
          description: 'Description',
          shareUrl: '',
          metadataSource: 'GameBase64',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/games',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.metadataSource()).toBe('GameBase64');
    });
  });

  describe('Chips Display', () => {
    it('should display meta1 chip when present', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.prg',
          path: '/test/test.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: '',
          meta1: 'PRG',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/test',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.meta1()).toBe('PRG');
      const chips = fixture.debugElement.queryAll(By.css('mat-chip'));
      expect(chips.length).toBe(1);
      expect(chips[0].nativeElement.textContent.trim()).toBe('PRG');
    });

    it('should display meta2 chip when present', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.prg',
          path: '/test/test.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: '',
          meta1: '',
          meta2: 'C64',
          metadataSourcePath: '',
          parentPath: '/test',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.meta2()).toBe('C64');
      const chips = fixture.debugElement.queryAll(By.css('mat-chip'));
      expect(chips.length).toBe(1);
      expect(chips[0].nativeElement.textContent.trim()).toBe('C64');
    });

    it('should display both chips when meta1 and meta2 are present', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.prg',
          path: '/test/test.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: '',
          meta1: 'PRG',
          meta2: 'C64',
          metadataSourcePath: '',
          parentPath: '/test',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      const chips = fixture.debugElement.queryAll(By.css('mat-chip'));
      expect(chips.length).toBe(2);
      expect(chips[0].nativeElement.textContent.trim()).toBe('PRG');
      expect(chips[1].nativeElement.textContent.trim()).toBe('C64');
    });

    it('should not display chip-set when meta1 and meta2 are empty', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.prg',
          path: '/test/test.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test',
          creator: '',
          releaseInfo: '',
          description: 'Test',
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
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      const chipSet = fixture.debugElement.query(By.css('mat-chip-set'));
      expect(chipSet).toBeNull();
    });
  });

  describe('Content States', () => {
    it('should show description when file has content', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.prg',
          path: '/test/test.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Game',
          creator: '',
          releaseInfo: '',
          description: 'This is a test game',
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
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      const description = fixture.debugElement.query(By.css('p:not(.no-metadata)'));
      expect(description).toBeTruthy();
      expect(description.nativeElement.textContent.trim()).toBe('This is a test game');
    });

    it('should show "no metadata" message when file has no title or description', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.prg',
          path: '/test/test.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
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
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.hasContent()).toBe(false);
      const noMetadata = fixture.debugElement.query(By.css('.no-metadata'));
      expect(noMetadata).toBeTruthy();
      expect(noMetadata.nativeElement.textContent.trim()).toBe('Try launching a file from the device.');
    });

    it('hasContent should return true when title exists even without description', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.prg',
          path: '/test/test.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Game',
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
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.hasContent()).toBe(true);
    });

    it('hasContent should return true when description exists even without title', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.prg',
          path: '/test/test.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: '',
          creator: '',
          releaseInfo: '',
          description: 'A description',
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
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.hasContent()).toBe(true);
    });
  });

  describe('Computed Signals', () => {
    it('should return empty strings for all metadata when no file is loaded', () => {
      currentFileSignal.set(null);
      fixture.detectChanges();

      expect(component.filename()).toBe('');
      expect(component.title()).toBe('');
      expect(component.releaseInfo()).toBe('');
      expect(component.description()).toBe('');
      expect(component.meta1()).toBe('');
      expect(component.meta2()).toBe('');
      expect(component.metadataSource()).toBe('');
    });

    it('should update computed values when file changes', () => {
      // Set initial file
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'file1.prg',
          path: '/test/file1.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'First Game',
          creator: '',
          releaseInfo: '1990',
          description: 'First description',
          shareUrl: '',
          metadataSource: 'Source1',
          meta1: 'PRG',
          meta2: 'C64',
          metadataSourcePath: '',
          parentPath: '/test',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.title()).toBe('First Game');
      expect(component.releaseInfo()).toBe('1990');

      // Change file
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'file2.prg',
          path: '/test/file2.prg',
          type: FileItemType.Game,
          size: 2048,
          isFavorite: false,
          isCompatible: true,
          title: 'Second Game',
          creator: '',
          releaseInfo: '1995',
          description: 'Second description',
          shareUrl: '',
          metadataSource: 'Source2',
          meta1: 'D64',
          meta2: 'VIC20',
          metadataSourcePath: '',
          parentPath: '/test',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.title()).toBe('Second Game');
      expect(component.releaseInfo()).toBe('1995');
      expect(component.meta1()).toBe('D64');
      expect(component.meta2()).toBe('VIC20');
    });
  });
});

