import { vi, describe, it, expect, beforeEach } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { PlayerToolbarComponent } from './player-toolbar.component';
import { PLAYER_CONTEXT, IPlayerContext } from '@teensyrom-nx/application';
import { LaunchMode, PlayerStatus, FileItemType } from '@teensyrom-nx/domain';

describe('PlayerToolbarComponent', () => {
  let component: PlayerToolbarComponent;
  let fixture: ComponentFixture<PlayerToolbarComponent>;
  let mockPlayerContext: IPlayerContext;

  // Helper to create file mock
  const createFileMock = (type: FileItemType) => ({
    name: 'test-file',
    path: '/test/test-file',
    type,
    size: 1024,
    isFavorite: false,
    title: 'Test File',
    creator: 'Test Creator',
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
  });

  beforeEach(async () => {
    // Create a proper interface-based mock that implements all IPlayerContext methods
    mockPlayerContext = {
      // Core player lifecycle
      initializePlayer: vi.fn(),
      removePlayer: vi.fn(),
      
      // File launching
      launchFileWithContext: vi.fn().mockResolvedValue(undefined),
      launchRandomFile: vi.fn().mockResolvedValue(undefined),
      
      // Phase 3: Playback control methods (tested in this component)
      play: vi.fn().mockResolvedValue(undefined),
      pause: vi.fn().mockResolvedValue(undefined),
      stop: vi.fn().mockResolvedValue(undefined),
      next: vi.fn().mockResolvedValue(undefined),
      previous: vi.fn().mockResolvedValue(undefined),
      
      // State queries
      getCurrentFile: vi.fn().mockReturnValue(signal(null).asReadonly()),
      getFileContext: vi.fn().mockReturnValue(signal(null).asReadonly()),
      getPlayerStatus: vi.fn().mockReturnValue(signal(PlayerStatus.Stopped).asReadonly()),
      getStatus: vi.fn().mockReturnValue(signal(PlayerStatus.Stopped).asReadonly()),
      isLoading: vi.fn().mockReturnValue(signal(false).asReadonly()),
      getError: vi.fn().mockReturnValue(signal(null).asReadonly()),
      
      // Shuffle functionality
      toggleShuffleMode: vi.fn(),
      setShuffleScope: vi.fn(),
      setFilterMode: vi.fn(),
      getLaunchMode: vi.fn().mockReturnValue(signal(LaunchMode.Directory).asReadonly()),
      getShuffleSettings: vi.fn().mockReturnValue(signal(null).asReadonly()),
    } satisfies IPlayerContext;

    await TestBed.configureTestingModule({
      imports: [PlayerToolbarComponent],
      providers: [
        provideNoopAnimations(),
        { provide: PLAYER_CONTEXT, useValue: mockPlayerContext },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PlayerToolbarComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'test-device-id');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Phase 3: Playback Control Integration', () => {
    const testDeviceId = 'test-device-id';

    describe('playPause() method', () => {
      it('should call pause when player is currently playing', async () => {
        mockPlayerContext.getPlayerStatus.mockReturnValue(signal(PlayerStatus.Playing).asReadonly());
        
        await component.playPause();
        
        expect(mockPlayerContext.pause).toHaveBeenCalledWith(testDeviceId);
        expect(mockPlayerContext.play).not.toHaveBeenCalled();
      });

      it('should call play when player is currently stopped', async () => {
        mockPlayerContext.getPlayerStatus.mockReturnValue(signal(PlayerStatus.Stopped).asReadonly());
        
        await component.playPause();
        
        expect(mockPlayerContext.play).toHaveBeenCalledWith(testDeviceId);
        expect(mockPlayerContext.pause).not.toHaveBeenCalled();
      });

      it('should call play when player is currently paused', async () => {
        mockPlayerContext.getPlayerStatus.mockReturnValue(signal(PlayerStatus.Paused).asReadonly());
        
        await component.playPause();
        
        expect(mockPlayerContext.play).toHaveBeenCalledWith(testDeviceId);
        expect(mockPlayerContext.pause).not.toHaveBeenCalled();
      });

      it('should not call play or pause when deviceId is empty', async () => {
        fixture.componentRef.setInput('deviceId', '');
        
        await component.playPause();
        
        expect(mockPlayerContext.play).not.toHaveBeenCalled();
        expect(mockPlayerContext.pause).not.toHaveBeenCalled();
      });
    });

    describe('stop() method', () => {
      it('should call playerContext.stop with correct deviceId', async () => {
        await component.stop();
        
        expect(mockPlayerContext.stop).toHaveBeenCalledWith(testDeviceId);
      });

      it('should not call stop when deviceId is empty', async () => {
        fixture.componentRef.setInput('deviceId', '');
        
        await component.stop();
        
        expect(mockPlayerContext.stop).not.toHaveBeenCalled();
      });
    });

    describe('next() method', () => {
      it('should call playerContext.next with correct deviceId', async () => {
        await component.next();
        
        expect(mockPlayerContext.next).toHaveBeenCalledWith(testDeviceId);
      });

      it('should not call next when deviceId is empty', async () => {
        fixture.componentRef.setInput('deviceId', '');
        
        await component.next();
        
        expect(mockPlayerContext.next).not.toHaveBeenCalled();
      });
    });

    describe('previous() method', () => {
      it('should call playerContext.previous with correct deviceId', async () => {
        await component.previous();
        
        expect(mockPlayerContext.previous).toHaveBeenCalledWith(testDeviceId);
      });

      it('should not call previous when deviceId is empty', async () => {
        fixture.componentRef.setInput('deviceId', '');
        
        await component.previous();
        
        expect(mockPlayerContext.previous).not.toHaveBeenCalled();
      });
    });
  });

  describe('UI Helper Methods', () => {
    describe('getPlayPauseIcon()', () => {
      it('should return "play_arrow" when status is Stopped', () => {
        mockPlayerContext.getPlayerStatus.mockReturnValue(signal(PlayerStatus.Stopped).asReadonly());
        
        expect(component.getPlayPauseIcon()).toBe('play_arrow');
      });

      it('should return "play_arrow" when status is Paused', () => {
        mockPlayerContext.getPlayerStatus.mockReturnValue(signal(PlayerStatus.Paused).asReadonly());
        
        expect(component.getPlayPauseIcon()).toBe('play_arrow');
      });

      it('should return "pause" when status is Playing', () => {
        mockPlayerContext.getPlayerStatus.mockReturnValue(signal(PlayerStatus.Playing).asReadonly());
        
        expect(component.getPlayPauseIcon()).toBe('pause');
      });

      it('should return "play_arrow" when deviceId is empty', () => {
        fixture.componentRef.setInput('deviceId', '');
        
        expect(component.getPlayPauseIcon()).toBe('play_arrow');
      });
    });

    describe('getPlayPauseLabel()', () => {
      it('should return "Play" when status is Stopped', () => {
        mockPlayerContext.getPlayerStatus.mockReturnValue(signal(PlayerStatus.Stopped).asReadonly());
        
        expect(component.getPlayPauseLabel()).toBe('Play');
      });

      it('should return "Play" when status is Paused', () => {
        mockPlayerContext.getPlayerStatus.mockReturnValue(signal(PlayerStatus.Paused).asReadonly());
        
        expect(component.getPlayPauseLabel()).toBe('Play');
      });

      it('should return "Pause" when status is Playing', () => {
        mockPlayerContext.getPlayerStatus.mockReturnValue(signal(PlayerStatus.Playing).asReadonly());
        
        expect(component.getPlayPauseLabel()).toBe('Pause');
      });

      it('should return "Play" when deviceId is empty', () => {
        fixture.componentRef.setInput('deviceId', '');
        
        expect(component.getPlayPauseLabel()).toBe('Play');
      });
    });

    describe('isCurrentFileMusicType()', () => {
      it('should return true when current file is a Song', () => {
        const musicFile = createFileMock(FileItemType.Song);
        mockPlayerContext.getCurrentFile.mockReturnValue(
          signal({ 
            storageKey: 'test-key',
            file: musicFile,
            parentPath: '/test',
            launchedAt: Date.now(),
            launchMode: LaunchMode.Directory
          }).asReadonly()
        );
        
        expect(component.isCurrentFileMusicType()).toBe(true);
      });

      it('should return false when current file is a Game', () => {
        const gameFile = createFileMock(FileItemType.Game);
        mockPlayerContext.getCurrentFile.mockReturnValue(
          signal({ 
            storageKey: 'test-key',
            file: gameFile,
            parentPath: '/test',
            launchedAt: Date.now(),
            launchMode: LaunchMode.Directory
          }).asReadonly()
        );
        
        expect(component.isCurrentFileMusicType()).toBe(false);
      });

      it('should return false when current file is an Image', () => {
        const imageFile = createFileMock(FileItemType.Image);
        mockPlayerContext.getCurrentFile.mockReturnValue(
          signal({ 
            storageKey: 'test-key',
            file: imageFile,
            parentPath: '/test',
            launchedAt: Date.now(),
            launchMode: LaunchMode.Directory
          }).asReadonly()
        );
        
        expect(component.isCurrentFileMusicType()).toBe(false);
      });

      it('should return false when no current file', () => {
        mockPlayerContext.getCurrentFile.mockReturnValue(signal(null).asReadonly());
        
        expect(component.isCurrentFileMusicType()).toBe(false);
      });

      it('should return false when deviceId is empty', () => {
        fixture.componentRef.setInput('deviceId', '');
        
        expect(component.isCurrentFileMusicType()).toBe(false);
      });
    });

    describe('canNavigate()', () => {
      it('should return true when file context has multiple files', () => {
        const files = [createFileMock(FileItemType.Song), createFileMock(FileItemType.Song)];
        mockPlayerContext.getFileContext.mockReturnValue(
          signal({
            storageKey: 'test-key',
            directoryPath: '/test',
            files,
            currentIndex: 0,
            launchMode: LaunchMode.Directory
          }).asReadonly()
        );
        
        expect(component.canNavigate()).toBe(true);
      });

      it('should return false when file context has only one file', () => {
        const files = [createFileMock(FileItemType.Song)];
        mockPlayerContext.getFileContext.mockReturnValue(
          signal({
            storageKey: 'test-key',
            directoryPath: '/test',
            files,
            currentIndex: 0,
            launchMode: LaunchMode.Directory
          }).asReadonly()
        );
        
        expect(component.canNavigate()).toBe(false);
      });

      it('should return true when in shuffle mode regardless of file context', () => {
        mockPlayerContext.getLaunchMode.mockReturnValue(signal(LaunchMode.Shuffle).asReadonly());
        mockPlayerContext.getFileContext.mockReturnValue(signal(null).asReadonly());
        
        expect(component.canNavigate()).toBe(true);
      });

      it('should return false when no file context and not in shuffle mode', () => {
        mockPlayerContext.getLaunchMode.mockReturnValue(signal(LaunchMode.Directory).asReadonly());
        mockPlayerContext.getFileContext.mockReturnValue(signal(null).asReadonly());
        
        expect(component.canNavigate()).toBe(false);
      });

      it('should return false when deviceId is empty', () => {
        fixture.componentRef.setInput('deviceId', '');
        
        expect(component.canNavigate()).toBe(false);
      });
    });

    describe('canNavigatePrevious()', () => {
      it('should use same logic as canNavigate()', () => {
        const files = [createFileMock(FileItemType.Song), createFileMock(FileItemType.Song)];
        mockPlayerContext.getFileContext.mockReturnValue(
          signal({
            storageKey: 'test-key',
            directoryPath: '/test',
            files,
            currentIndex: 0,
            launchMode: LaunchMode.Directory
          }).asReadonly()
        );
        
        expect(component.canNavigatePrevious()).toBe(component.canNavigate());
        expect(component.canNavigatePrevious()).toBe(true);
      });
    });

    describe('getPlayerStatus()', () => {
      it('should return current player status from context', () => {
        mockPlayerContext.getPlayerStatus.mockReturnValue(signal(PlayerStatus.Playing).asReadonly());
        
        expect(component.getPlayerStatus()).toBe(PlayerStatus.Playing);
      });

      it('should return Stopped when deviceId is empty', () => {
        fixture.componentRef.setInput('deviceId', '');
        
        expect(component.getPlayerStatus()).toBe(PlayerStatus.Stopped);
      });
    });
  });

  describe('Template Integration', () => {
    beforeEach(() => {
      // Set up a mock file context for button state tests
      const musicFile = createFileMock(FileItemType.Song);
      mockPlayerContext.getCurrentFile.mockReturnValue(
        signal({ 
          storageKey: 'test-key',
          file: musicFile,
          parentPath: '/test',
          launchedAt: Date.now(),
          launchMode: LaunchMode.Directory
        }).asReadonly()
      );
    });

    it('should show play/pause button for music files', () => {
      fixture.detectChanges();
      
      const playPauseButton = fixture.debugElement.nativeElement.querySelector('[aria-label*="Play"], [aria-label*="Pause"]');
      expect(playPauseButton).toBeTruthy();
    });

    it('should show stop button for non-music files', () => {
      const gameFile = createFileMock(FileItemType.Game);
      mockPlayerContext.getCurrentFile.mockReturnValue(
        signal({ 
          storageKey: 'test-key',
          file: gameFile,
          parentPath: '/test',
          launchedAt: Date.now(),
          launchMode: LaunchMode.Directory
        }).asReadonly()
      );
      
      fixture.detectChanges();
      
      const stopButton = fixture.debugElement.nativeElement.querySelector('[aria-label="Stop Playback"]');
      expect(stopButton).toBeTruthy();
    });

    it('should disable navigation buttons when canNavigate returns false', () => {
      mockPlayerContext.getFileContext.mockReturnValue(signal(null).asReadonly());
      mockPlayerContext.getLaunchMode.mockReturnValue(signal(LaunchMode.Directory).asReadonly());
      
      fixture.detectChanges();
      
      const nextButton = fixture.debugElement.nativeElement.querySelector('[aria-label="Next File"]');
      const previousButton = fixture.debugElement.nativeElement.querySelector('[aria-label="Previous File"]');
      
      expect(nextButton.disabled).toBe(true);
      expect(previousButton.disabled).toBe(true);
    });

    it('should enable navigation buttons when canNavigate returns true', () => {
      const files = [createFileMock(FileItemType.Song), createFileMock(FileItemType.Song)];
      mockPlayerContext.getFileContext.mockReturnValue(
        signal({
          storageKey: 'test-key',
          directoryPath: '/test',
          files,
          currentIndex: 0,
          launchMode: LaunchMode.Directory
        }).asReadonly()
      );
      
      fixture.detectChanges();
      
      const nextButton = fixture.debugElement.nativeElement.querySelector('[aria-label="Next File"]');
      const previousButton = fixture.debugElement.nativeElement.querySelector('[aria-label="Previous File"]');
      
      expect(nextButton.disabled).toBe(false);
      expect(previousButton.disabled).toBe(false);
    });

    // Note: Buttons are not currently disabled during loading state
    // They maintain their normal enabled/disabled state based on canNavigate() etc.
    // If loading state should disable buttons, add [disabled]="isLoading()" to template
  });

  describe('Button Click Integration', () => {
    it('should trigger playPause when play/pause button is clicked', async () => {
      const spy = vi.spyOn(component, 'playPause');
      
      // Set up music file to show play/pause button
      const musicFile = createFileMock(FileItemType.Song);
      mockPlayerContext.getCurrentFile.mockReturnValue(
        signal({ 
          storageKey: 'test-key',
          file: musicFile,
          parentPath: '/test',
          launchedAt: Date.now(),
          launchMode: LaunchMode.Directory
        }).asReadonly()
      );
      
      fixture.detectChanges();
      
      const playPauseButton = fixture.debugElement.nativeElement.querySelector('[aria-label*="Play"], [aria-label*="Pause"]');
      playPauseButton.click();
      
      expect(spy).toHaveBeenCalled();
    });

    it('should trigger next when next button is clicked', async () => {
      const spy = vi.spyOn(component, 'next');

      // Need to have a file loaded for toolbar to be visible
      const musicFile = createFileMock(FileItemType.Song);
      mockPlayerContext.getCurrentFile.mockReturnValue(
        signal({
          storageKey: 'test-key',
          file: musicFile,
          parentPath: '/test',
          launchedAt: Date.now(),
          launchMode: LaunchMode.Directory
        }).asReadonly()
      );

      // Enable navigation
      const files = [createFileMock(FileItemType.Song), createFileMock(FileItemType.Song)];
      mockPlayerContext.getFileContext.mockReturnValue(
        signal({
          storageKey: 'test-key',
          directoryPath: '/test',
          files,
          currentIndex: 0,
          launchMode: LaunchMode.Directory
        }).asReadonly()
      );

      fixture.detectChanges();

      const nextButton = fixture.debugElement.nativeElement.querySelector('[aria-label="Next File"]');
      nextButton.click();

      expect(spy).toHaveBeenCalled();
    });

    it('should trigger previous when previous button is clicked', async () => {
      const spy = vi.spyOn(component, 'previous');

      // Need to have a file loaded for toolbar to be visible
      const musicFile = createFileMock(FileItemType.Song);
      mockPlayerContext.getCurrentFile.mockReturnValue(
        signal({
          storageKey: 'test-key',
          file: musicFile,
          parentPath: '/test',
          launchedAt: Date.now(),
          launchMode: LaunchMode.Directory
        }).asReadonly()
      );

      // Enable navigation
      const files = [createFileMock(FileItemType.Song), createFileMock(FileItemType.Song)];
      mockPlayerContext.getFileContext.mockReturnValue(
        signal({
          storageKey: 'test-key',
          directoryPath: '/test',
          files,
          currentIndex: 0,
          launchMode: LaunchMode.Directory
        }).asReadonly()
      );

      fixture.detectChanges();

      const previousButton = fixture.debugElement.nativeElement.querySelector('[aria-label="Previous File"]');
      previousButton.click();

      expect(spy).toHaveBeenCalled();
    });
  });
});
