import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal, WritableSignal } from '@angular/core';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { PlayHistoryComponent } from './play-history.component';
import { PLAYER_CONTEXT, IPlayerContext, PlayHistory, HistoryEntry, LaunchedFile, StorageKeyUtil } from '@teensyrom-nx/application';
import { FileItemType, StorageType, LaunchMode } from '@teensyrom-nx/domain';

describe('PlayHistoryComponent', () => {
  let component: PlayHistoryComponent;
  let fixture: ComponentFixture<PlayHistoryComponent>;
  let mockPlayerContext: Partial<IPlayerContext>;

  let playHistorySignal: WritableSignal<PlayHistory | null>;
  let currentFileSignal: WritableSignal<LaunchedFile | null>;
  let errorSignal: WritableSignal<string | null>;

  const createMockHistoryEntry = (name: string, timestamp: number): HistoryEntry => ({
    file: {
      path: `/test/${name}`,
      name,
      size: 1024,
      type: FileItemType.Song,
      images: [],
      parentPath: '/test',
      description: '',
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
    },
    storageKey: StorageKeyUtil.create('test-device', StorageType.Usb),
    parentPath: '/test',
    launchMode: LaunchMode.Shuffle,
    timestamp,
    isCompatible: true,
  });

  beforeEach(async () => {
    // Create signal mocks
    playHistorySignal = signal<PlayHistory | null>(null);
    currentFileSignal = signal<LaunchedFile | null>(null);
    errorSignal = signal<string | null>(null);

    // Create mock PlayerContext using IPlayerContext interface
    mockPlayerContext = {
      getPlayHistory: () => playHistorySignal.asReadonly(),
      getCurrentFile: () => currentFileSignal.asReadonly(),
      getError: () => errorSignal.asReadonly(),
      navigateToHistoryPosition: vi.fn(),
    } as Partial<IPlayerContext>;

    await TestBed.configureTestingModule({
      imports: [PlayHistoryComponent],
      providers: [
        provideNoopAnimations(),
        { provide: PLAYER_CONTEXT, useValue: mockPlayerContext },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PlayHistoryComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'test-device');
  });

  describe('Component Rendering', () => {
    it('should create component successfully', () => {
      expect(component).toBeTruthy();
    });

    it('should display empty state when no history exists', () => {
      playHistorySignal.set(null);
      fixture.detectChanges();

      const emptyStateElement = fixture.nativeElement.querySelector('lib-empty-state-message');
      expect(emptyStateElement).toBeTruthy();
      expect(fixture.nativeElement.textContent).toContain('No Play History');
    });

    it('should display history entries when history exists', () => {
      const entries = [
        createMockHistoryEntry('file1.sid', Date.now() - 3000),
        createMockHistoryEntry('file2.sid', Date.now() - 2000),
        createMockHistoryEntry('file3.sid', Date.now() - 1000),
      ];

      playHistorySignal.set({
        entries,
        currentPosition: -1,
      });

      fixture.detectChanges();

      const historyList = fixture.nativeElement.querySelector('.history-list');
      expect(historyList).toBeTruthy();

      const historyItems = fixture.nativeElement.querySelectorAll('lib-history-entry');
      expect(historyItems.length).toBe(3);
    });

    it('should display entries in reverse chronological order (newest first)', () => {
      const timestamp1 = Date.now() - 3000;
      const timestamp2 = Date.now() - 2000;
      const timestamp3 = Date.now() - 1000;

      const entries = [
        createMockHistoryEntry('oldest.sid', timestamp1),
        createMockHistoryEntry('middle.sid', timestamp2),
        createMockHistoryEntry('newest.sid', timestamp3),
      ];

      playHistorySignal.set({
        entries,
        currentPosition: -1,
      });

      fixture.detectChanges();

      const historyItems = fixture.nativeElement.querySelectorAll('.history-list-item');
      
      // First item should be newest (reversed) - check by index
      expect(historyItems[0].getAttribute('data-index')).toBe('0');
      expect(historyItems.length).toBe(3);
      // Last item should be oldest
      expect(historyItems[2].getAttribute('data-index')).toBe('2');
    });

    it('should highlight currently playing entry', () => {
      const entries = [
        createMockHistoryEntry('file1.sid', 1000),
        createMockHistoryEntry('file2.sid', 2000),
      ];

      playHistorySignal.set({
        entries,
        currentPosition: 1,
      });

      currentFileSignal.set({
        storageKey: StorageKeyUtil.create('test-device', StorageType.Usb),
        file: entries[1].file,
        parentPath: '/test',
        launchedAt: 2000,
        launchMode: LaunchMode.Shuffle,
        isCompatible: true,
      });

      fixture.detectChanges();

      const historyItems = fixture.nativeElement.querySelectorAll('.history-list-item');
      // First item in display (reversed order) should be playing
      expect(historyItems[0].getAttribute('data-is-playing')).toBe('true');
    });

    it('should show error highlight when current file has error', () => {
      const entry = createMockHistoryEntry('error-file.sid', 1000);

      playHistorySignal.set({
        entries: [entry],
        currentPosition: 0,
      });

      currentFileSignal.set({
        storageKey: StorageKeyUtil.create('test-device', StorageType.Usb),
        file: entry.file,
        parentPath: '/test',
        launchedAt: 1000,
        launchMode: LaunchMode.Shuffle,
        isCompatible: true,
      });

      errorSignal.set('Failed to launch file');

      fixture.detectChanges();

      const historyItem = fixture.nativeElement.querySelector('.history-list-item');
      expect(historyItem.getAttribute('data-is-playing')).toBe('true');
      expect(historyItem.getAttribute('data-has-error')).toBe('true');
    });
  });

  describe('User Interactions', () => {
    it('should select entry on single click', () => {
      const entries = [
        createMockHistoryEntry('file1.sid', 1000),
        createMockHistoryEntry('file2.sid', 2000),
      ];

      playHistorySignal.set({
        entries,
        currentPosition: -1,
      });

      fixture.detectChanges();

      const entry = entries[1];
      component.onEntrySelected(entry);

      expect(component.selectedEntry()).toEqual(entry);
    });

    it('should navigate to history position on double click', async () => {
      const entries = [
        createMockHistoryEntry('file1.sid', 1000),
        createMockHistoryEntry('file2.sid', 2000),
        createMockHistoryEntry('file3.sid', 3000),
      ];

      playHistorySignal.set({
        entries,
        currentPosition: -1,
      });

      fixture.detectChanges();

      // Double-click the first displayed item (newest, which is index 2 in original array)
      await component.onEntryDoubleClick(entries[2], 0);

      expect(mockPlayerContext.navigateToHistoryPosition).toHaveBeenCalledWith('test-device', 2);
    });

    it('should calculate correct history position for reversed display', async () => {
      const entries = [
        createMockHistoryEntry('oldest.sid', 1000),
        createMockHistoryEntry('middle.sid', 2000),
        createMockHistoryEntry('newest.sid', 3000),
      ];

      playHistorySignal.set({
        entries,
        currentPosition: -1,
      });

      fixture.detectChanges();

      // Click the last displayed item (oldest, which is index 0 in original array)
      await component.onEntryDoubleClick(entries[0], 2);

      expect(mockPlayerContext.navigateToHistoryPosition).toHaveBeenCalledWith('test-device', 0);
    });

    it('should not navigate if history is empty', async () => {
      playHistorySignal.set({
        entries: [],
        currentPosition: -1,
      });

      fixture.detectChanges();

      const mockEntry = createMockHistoryEntry('file.sid', 1000);
      await component.onEntryDoubleClick(mockEntry, 0);

      expect(mockPlayerContext.navigateToHistoryPosition).not.toHaveBeenCalled();
    });
  });

  describe('Selection State', () => {
    it('should identify selected entry correctly', () => {
      const entry1 = createMockHistoryEntry('file1.sid', 1000);
      const entry2 = createMockHistoryEntry('file2.sid', 2000);

      component.selectedEntry.set(entry1);

      expect(component.isSelected(entry1)).toBe(true);
      expect(component.isSelected(entry2)).toBe(false);
    });

    it('should identify currently playing entry correctly', () => {
      const entry1 = createMockHistoryEntry('file1.sid', 1000);
      const entry2 = createMockHistoryEntry('file2.sid', 2000);

      currentFileSignal.set({
        storageKey: StorageKeyUtil.create('test-device', StorageType.Usb),
        file: entry1.file,
        parentPath: '/test',
        launchedAt: 1000,
        launchMode: LaunchMode.Shuffle,
        isCompatible: true,
      });

      fixture.detectChanges();

      expect(component.isCurrentlyPlaying(entry1)).toBe(true);
      expect(component.isCurrentlyPlaying(entry2)).toBe(false);
    });
  });
});
