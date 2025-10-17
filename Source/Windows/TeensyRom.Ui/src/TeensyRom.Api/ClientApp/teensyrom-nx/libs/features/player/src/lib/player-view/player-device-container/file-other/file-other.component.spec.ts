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
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [],
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
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [],
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

  describe('DeepSID Metadata - Links', () => {
    it('should display links section when links exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: 'Test Artist',
          releaseInfo: '2024',
          description: 'A test song',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [
            { name: 'CSDb', url: 'https://csdb.dk/release/?id=12345' },
            { name: 'Pouët', url: 'https://pouet.net/prod.php?which=12345' }
          ],
          tags: [],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.links().length).toBe(2);
      
      const linksSection = fixture.debugElement.query(By.css('.links-section'));
      expect(linksSection).toBeTruthy();
      
      const heading = linksSection.query(By.css('h4'));
      expect(heading.nativeElement.textContent).toBe('Links');
      
      const links = linksSection.queryAll(By.css('lib-external-link'));
      expect(links.length).toBe(2);
      expect(links[0].nativeElement.textContent.trim()).toContain('CSDb');
      expect(links[1].nativeElement.textContent.trim()).toContain('Pouët');
    });

    it('should not display links section when no links exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'A test song',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.links().length).toBe(0);
      const linksSection = fixture.debugElement.query(By.css('.links-section'));
      expect(linksSection).toBeNull();
    });

    it('should open links in new tab', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [{ name: 'Test Link', url: 'https://example.com' }],
          tags: [],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      const link = fixture.debugElement.query(By.css('lib-external-link'));
      // External link component defaults to target="_blank"
      expect(link).toBeTruthy();
    });
  });

  describe('DeepSID Metadata - YouTube Videos', () => {
    it('should display YouTube videos section when videos exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [
            { videoId: 'abc123', url: 'https://youtube.com/watch?v=abc123', channel: 'C64 Music Channel', subtune: 0 },
            { videoId: 'def456', url: 'https://youtube.com/watch?v=def456', channel: 'Retro Gaming', subtune: 2 }
          ],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.youTubeVideos().length).toBe(2);
      
      const videosSection = fixture.debugElement.query(By.css('.youtube-section'));
      expect(videosSection).toBeTruthy();
      
      const heading = videosSection.query(By.css('h4'));
      expect(heading.nativeElement.textContent).toBe('Related Videos');
      
      const videos = videosSection.queryAll(By.css('lib-external-link'));
      expect(videos.length).toBe(2);
      expect(videos[0].nativeElement.textContent.trim()).toContain('C64 Music Channel');
      expect(videos[1].nativeElement.textContent.trim()).toContain('Retro Gaming');
    });

    it('should display subtune information for videos with subtune > 0', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [
            { videoId: 'xyz789', url: 'https://youtube.com/watch?v=xyz789', channel: 'SID Channel', subtune: 3 }
          ],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      const videoLink = fixture.debugElement.query(By.css('lib-external-link'));
      // The component uses channel label directly; subtune info was in old template
      expect(videoLink).toBeTruthy();
      expect(videoLink.nativeElement.textContent.trim()).toContain('SID Channel');
    });

    it('should not display subtune information for videos with subtune = 0', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [
            { videoId: 'xyz789', url: 'https://youtube.com/watch?v=xyz789', channel: 'SID Channel', subtune: 0 }
          ],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      const videoLink = fixture.debugElement.query(By.css('lib-external-link'));
      // External link component displays channel label with icon text
      expect(videoLink).toBeTruthy();
      expect(videoLink.nativeElement.textContent).toContain('SID Channel');
    });

    it('should not display YouTube section when no videos exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.youTubeVideos().length).toBe(0);
      const videosSection = fixture.debugElement.query(By.css('.youtube-section'));
      expect(videosSection).toBeNull();
    });
  });

  describe('DeepSID Metadata - Competitions', () => {
    it('should display competitions section when competitions exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [
            { name: 'X Party 2024', place: 1 },
            { name: 'Demo Scene Awards' }
          ],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.competitions().length).toBe(2);
      
      const competitionsSection = fixture.debugElement.query(By.css('.competitions-section'));
      expect(competitionsSection).toBeTruthy();
      
      const heading = competitionsSection.query(By.css('h4'));
      expect(heading.nativeElement.textContent).toBe('Competition Results');
      
      const items = competitionsSection.queryAll(By.css('.competition-item'));
      expect(items.length).toBe(2);
      expect(items[0].nativeElement.textContent.trim()).toContain('X Party 2024');
      expect(items[0].nativeElement.textContent.trim()).toContain('Place 1');
    });

    it('should display competition without place when place is undefined', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [
            { name: 'Demo Scene Awards' }
          ],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      const competitionItem = fixture.debugElement.query(By.css('.competition-item'));
      expect(competitionItem.nativeElement.textContent.trim()).toBe('Demo Scene Awards');
      
      const position = competitionItem.query(By.css('.position'));
      expect(position).toBeNull();
    });

    it('should not display competitions section when no competitions exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.competitions().length).toBe(0);
      const competitionsSection = fixture.debugElement.query(By.css('.competitions-section'));
      expect(competitionsSection).toBeNull();
    });
  });

  describe('DeepSID Metadata - Tags', () => {
    it('should display tags section when tags exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [
            { name: 'Chiptune', type: 'genre' },
            { name: 'Classic', type: 'era' }
          ],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.tags().length).toBe(2);
      
      const tagsSection = fixture.debugElement.query(By.css('.tags-section'));
      expect(tagsSection).toBeTruthy();
      
      const heading = tagsSection.query(By.css('h4'));
      expect(heading.nativeElement.textContent).toBe('Tags');
      
      const chipSet = tagsSection.query(By.css('mat-chip-set'));
      expect(chipSet).toBeTruthy();
      
      const chips = chipSet.queryAll(By.css('mat-chip'));
      expect(chips.length).toBe(2);
      expect(chips[0].nativeElement.textContent.trim()).toBe('Chiptune');
      expect(chips[0].nativeElement.classList.contains('tag-genre')).toBe(true);
      expect(chips[1].nativeElement.textContent.trim()).toBe('Classic');
      expect(chips[1].nativeElement.classList.contains('tag-era')).toBe(true);
    });

    it('should not display tags section when no tags exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.tags().length).toBe(0);
      const tagsSection = fixture.debugElement.query(By.css('.tags-section'));
      expect(tagsSection).toBeNull();
    });
  });

  describe('DeepSID Metadata - Ratings', () => {
    it('should display rating section when avgRating exists', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [],
          avgRating: 4.5,
          ratingCount: 42,
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.avgRating()).toBe(4.5);
      expect(component.ratingCount()).toBe(42);
      
      const ratingSection = fixture.debugElement.query(By.css('.rating-section'));
      expect(ratingSection).toBeTruthy();
      expect(ratingSection.nativeElement.textContent).toContain('4.5/5.0');
      expect(ratingSection.nativeElement.textContent).toContain('(42 ratings)');
    });

    it('should not display rating section when avgRating is undefined', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.avgRating()).toBeUndefined();
      const ratingSection = fixture.debugElement.query(By.css('.rating-section'));
      expect(ratingSection).toBeNull();
    });
  });

  describe('HVSC STIL Title for Songs', () => {
    it('should display "HVSC STIL" title for Song file type with description', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'STIL information about this song',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.isSong()).toBe(true);
      
      const descriptionSection = fixture.debugElement.query(By.css('.description-section'));
      expect(descriptionSection).toBeTruthy();
      
      const sectionTitle = descriptionSection.query(By.css('.section-title'));
      expect(sectionTitle).toBeTruthy();
      expect(sectionTitle.nativeElement.textContent.trim()).toBe('HVSC STIL');
    });

    it('should not display "HVSC STIL" title for non-Song file types', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.prg',
          path: '/games/test.prg',
          type: FileItemType.Game,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Game',
          creator: '',
          releaseInfo: '',
          description: 'Game description',
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
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.isSong()).toBe(false);
      
      const sectionTitle = fixture.debugElement.query(By.css('.section-title'));
      expect(sectionTitle).toBeNull();
    });

    it('should not display HVSC STIL section when Song has no description', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: '',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.isSong()).toBe(true);
      expect(component.description()).toBe('');
      
      const descriptionSection = fixture.debugElement.query(By.css('.description-section'));
      expect(descriptionSection).toBeNull();
    });
  });

  describe('Metadata Grid Layout', () => {
    it('should display metadata-grid when sections exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [{ name: 'Link', url: 'http://example.com' }],
          tags: [{ name: 'Tag', type: 'genre' }],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      const metadataGrid = fixture.debugElement.query(By.css('.metadata-grid'));
      expect(metadataGrid).toBeTruthy();
    });

    it('should display all sections in metadata grid when data is available', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: 'Test Song',
          creator: '',
          releaseInfo: '',
          description: 'Test',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [{ name: 'Link', url: 'http://example.com' }],
          tags: [{ name: 'Tag', type: 'genre' }],
          youTubeVideos: [{ videoId: 'abc', url: 'http://youtube.com', channel: 'Channel', subtune: 0 }],
          competitions: [{ name: 'Competition', place: 1 }],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      const linksSection = fixture.debugElement.query(By.css('.links-section'));
      const videosSection = fixture.debugElement.query(By.css('.youtube-section'));
      const competitionsSection = fixture.debugElement.query(By.css('.competitions-section'));
      const tagsSection = fixture.debugElement.query(By.css('.tags-section'));

      expect(linksSection).toBeTruthy();
      expect(videosSection).toBeTruthy();
      expect(competitionsSection).toBeTruthy();
      expect(tagsSection).toBeTruthy();
    });
  });

  describe('hasContent Computed Signal with DeepSID Data', () => {
    it('should return true when links exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: '',
          creator: '',
          releaseInfo: '',
          description: '',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [{ name: 'Link', url: 'http://example.com' }],
          tags: [],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.hasContent()).toBe(true);
    });

    it('should return true when tags exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: '',
          creator: '',
          releaseInfo: '',
          description: '',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [{ name: 'Tag', type: 'genre' }],
          youTubeVideos: [],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.hasContent()).toBe(true);
    });

    it('should return true when YouTube videos exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: '',
          creator: '',
          releaseInfo: '',
          description: '',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [{ videoId: 'abc', url: 'http://youtube.com', channel: 'Channel', subtune: 0 }],
          competitions: [],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.hasContent()).toBe(true);
    });

    it('should return true when competitions exist', () => {
      currentFileSignal.set({
        deviceId: 1,
        storageType: StorageType.Sd,
        file: {
          name: 'test.sid',
          path: '/music/test.sid',
          type: FileItemType.Song,
          size: 1024,
          isFavorite: false,
          isCompatible: true,
          title: '',
          creator: '',
          releaseInfo: '',
          description: '',
          shareUrl: '',
          metadataSource: 'DeepSID',
          meta1: '',
          meta2: '',
          metadataSourcePath: '',
          parentPath: '/music',
          playLength: '',
          subtuneLengths: [],
          startSubtuneNum: 0,
          images: [],
          links: [],
          tags: [],
          youTubeVideos: [],
          competitions: [{ name: 'Competition' }],
        },
        isShuffleMode: false,
      });
      fixture.detectChanges();

      expect(component.hasContent()).toBe(true);
    });
  });
});

