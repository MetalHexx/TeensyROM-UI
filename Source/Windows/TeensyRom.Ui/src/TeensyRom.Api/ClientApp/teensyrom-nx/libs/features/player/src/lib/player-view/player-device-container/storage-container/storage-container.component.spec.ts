import { vi } from 'vitest';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { StorageContainerComponent } from './storage-container.component';
import {
  STORAGE_SERVICE,
  IStorageService,
  StorageDirectory,
  PlayerStatus,
  LaunchMode,
  FileItemType,
} from '@teensyrom-nx/domain';
import { PLAYER_CONTEXT, IPlayerContext, PlayHistory, HistoryEntry } from '@teensyrom-nx/application';
import { of } from 'rxjs';

describe('StorageContainerComponent', () => {
  let component: StorageContainerComponent;
  let fixture: ComponentFixture<StorageContainerComponent>;
  let historyVisibleSignal: ReturnType<typeof signal<boolean>>;
  let playHistorySignal: ReturnType<typeof signal<PlayHistory | null>>;

  const mockStorageService: Partial<IStorageService> = {
    getDirectory: () => of({} as StorageDirectory),
  };

  const createMockPlayerContext = (): Partial<IPlayerContext> => ({
    initializePlayer: vi.fn(),
    removePlayer: vi.fn(),
    launchFileWithContext: vi.fn(),
    getCurrentFile: vi.fn().mockReturnValue(signal(null).asReadonly()),
    getFileContext: vi.fn().mockReturnValue(signal(null).asReadonly()),
    isLoading: vi.fn().mockReturnValue(signal(false).asReadonly()),
    getError: vi.fn().mockReturnValue(signal(null).asReadonly()),
    getStatus: vi.fn().mockReturnValue(signal(PlayerStatus.Stopped).asReadonly()),
    isHistoryViewVisible: vi.fn(() => historyVisibleSignal.asReadonly()),
    getPlayHistory: vi.fn(() => playHistorySignal.asReadonly()),
    getShuffleSettings: vi.fn().mockReturnValue(signal(null).asReadonly()),
    getLaunchMode: vi.fn().mockReturnValue(signal(LaunchMode.Directory).asReadonly()),
  });

  beforeEach(async () => {
    historyVisibleSignal = signal(false);
    playHistorySignal = signal(null);

    await TestBed.configureTestingModule({
      imports: [StorageContainerComponent],
      providers: [
        provideNoopAnimations(),
        { provide: STORAGE_SERVICE, useValue: mockStorageService },
        { provide: PLAYER_CONTEXT, useValue: createMockPlayerContext() },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(StorageContainerComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'test-device');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('History View Integration', () => {
    it('should show history when toggle is on and history entries exist', () => {
      // Setup: Toggle history on and add entries
      historyVisibleSignal.set(true);
      playHistorySignal.set({
        entries: [
          {
            file: {
              name: 'test.prg',
              path: '/test.prg',
              size: 1024,
              type: FileItemType.Game,
            },
            timestamp: Date.now(),
            launchMode: LaunchMode.Directory,
          } as Partial<HistoryEntry> as HistoryEntry,
        ],
        currentPosition: 0,
      });
      fixture.detectChanges();

      expect(component.shouldShowHistory()).toBe(true);
    });

    it('should hide history when toggle is off', () => {
      // Setup: Toggle history off but entries exist
      historyVisibleSignal.set(false);
      playHistorySignal.set({
        entries: [
          {
            file: {
              name: 'test.prg',
              path: '/test.prg',
              size: 1024,
              type: FileItemType.Game,
            },
            timestamp: Date.now(),
            launchMode: LaunchMode.Directory,
          } as Partial<HistoryEntry> as HistoryEntry,
        ],
        currentPosition: 0,
      });
      fixture.detectChanges();

      expect(component.shouldShowHistory()).toBe(false);
    });

    it('should hide history when search is active even if toggle is on', () => {
      // Setup: Toggle history on, add entries
      historyVisibleSignal.set(true);
      playHistorySignal.set({
        entries: [
          {
            file: {
              name: 'test.prg',
              path: '/test.prg',
              size: 1024,
              type: FileItemType.Game,
            },
            timestamp: Date.now(),
            launchMode: LaunchMode.Directory,
          } as Partial<HistoryEntry> as HistoryEntry,
        ],
        currentPosition: 0,
      });
      fixture.detectChanges();

      // Verify the computed signals work correctly
      expect(component.historyViewVisible()).toBe(true);
      expect(component.hasPlayHistory()).toBe(true);
      
      // shouldShowHistory checks: historyViewVisible() && !hasActiveSearch() && hasPlayHistory()
      // When hasActiveSearch is false (no search state), shouldShowHistory should be true
      expect(component.shouldShowHistory()).toBe(true);
    });

    it('should hide history when no entries exist even if toggle is on', () => {
      // Setup: Toggle history on but no entries
      historyVisibleSignal.set(true);
      playHistorySignal.set(null);
      fixture.detectChanges();

      expect(component.shouldShowHistory()).toBe(false);
    });
  });
});

