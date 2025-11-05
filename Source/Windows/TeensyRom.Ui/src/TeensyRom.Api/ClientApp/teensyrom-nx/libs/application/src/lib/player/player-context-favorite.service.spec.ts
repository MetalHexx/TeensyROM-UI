import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { of, Subject } from 'rxjs';
import {
  DEVICE_SERVICE,
  FileItem,
  FileItemType,
  IDeviceService,
  IPlayerService,
  LaunchMode,
  PLAYER_SERVICE,
  StorageType,
} from '@teensyrom-nx/domain';
import { PlayerContextService } from './player-context.service';
import { PlayerStore } from './player-store';
import { StorageStore } from '../storage/storage-store';
import { PlayerTimerManager } from './player-timer-manager';

type StorageStoreContract = Partial<typeof StorageStore>;

const createTestFile = (overrides: Partial<FileItem> = {}): FileItem => ({
  name: 'test-file.sid',
  path: '/music/test-file.sid',
  size: 4096,
  type: FileItemType.Song,
  isFavorite: false,
  isCompatible: true,
  title: 'Test Track',
  creator: 'Composer',
  releaseInfo: '2025',
  description: 'Test description',
  shareUrl: '',
  metadataSource: '',
  meta1: '',
  meta2: '',
  metadataSourcePath: '',
  parentPath: '/music',
  playLength: '3:00',
  subtuneLengths: [],
  startSubtuneNum: 0,
  images: [],
  links: [],
  tags: [],
  youTubeVideos: [],
  competitions: [],
  ratingCount: 0,
  ...overrides,
});

describe('PlayerContextService - Favorite Synchronization', () => {
  let service: PlayerContextService;
  let mockPlayerService: Partial<IPlayerService>;
  let mockDeviceService: Partial<IDeviceService>;
  let mockStorageStore: Partial<StorageStoreContract>;
  let mockTimerManager: Partial<PlayerTimerManager>;
  let timerUpdateSubject: Subject<unknown>;
  let timerCompleteSubject: Subject<void>;

  const deviceId = 'device-favorite-test';
  const storageType = StorageType.Sd;
  beforeEach(() => {
    timerUpdateSubject = new Subject();
    timerCompleteSubject = new Subject<void>();

    mockPlayerService = {
      launchFile: vi.fn<IPlayerService['launchFile']>().mockReturnValue(of(createTestFile())),
      launchRandom: vi.fn<IPlayerService['launchRandom']>(),
      toggleMusic: vi.fn<IPlayerService['toggleMusic']>(),
    };

    mockDeviceService = {
      findDevices: vi.fn<IDeviceService['findDevices']>(),
      getConnectedDevices: vi.fn<IDeviceService['getConnectedDevices']>(),
      connectDevice: vi.fn<IDeviceService['connectDevice']>(),
      disconnectDevice: vi.fn<IDeviceService['disconnectDevice']>(),
      resetDevice: vi.fn<IDeviceService['resetDevice']>(),
      pingDevice: vi.fn<IDeviceService['pingDevice']>(),
    };

    mockStorageStore = {
      navigateToDirectory: vi
        .fn<StorageStoreContract['navigateToDirectory']>()
        .mockResolvedValue(undefined),
      getSelectedDirectoryState: vi.fn<StorageStoreContract['getSelectedDirectoryState']>(),
    };

    mockTimerManager = {
      createTimer: vi.fn<PlayerTimerManager['createTimer']>(),
      destroyTimer: vi.fn<PlayerTimerManager['destroyTimer']>(),
      pauseTimer: vi.fn<PlayerTimerManager['pauseTimer']>(),
      resumeTimer: vi.fn<PlayerTimerManager['resumeTimer']>(),
      stopTimer: vi.fn<PlayerTimerManager['stopTimer']>(),
      setSpeed: vi.fn<PlayerTimerManager['setSpeed']>(),
      getTimerState: vi.fn<PlayerTimerManager['getTimerState']>(),
      onTimerUpdate$: vi
        .fn<PlayerTimerManager['onTimerUpdate$']>()
        .mockReturnValue(timerUpdateSubject.asObservable()),
      onTimerComplete$: vi
        .fn<PlayerTimerManager['onTimerComplete$']>()
        .mockReturnValue(timerCompleteSubject.asObservable()),
    };

    TestBed.configureTestingModule({
      providers: [
        PlayerContextService,
        PlayerStore,
        { provide: PLAYER_SERVICE, useValue: mockPlayerService },
        { provide: DEVICE_SERVICE, useValue: mockDeviceService },
        { provide: StorageStore, useValue: mockStorageStore },
        { provide: PlayerTimerManager, useValue: mockTimerManager },
      ],
    });

    service = TestBed.inject(PlayerContextService);
  });

  const launchFileForDevice = async (file: FileItem, directoryFiles: FileItem[]) => {
    service.initializePlayer(deviceId);
    const launchFileMock = mockPlayerService.launchFile as ReturnType<
      typeof vi.fn<IPlayerService['launchFile']>
    >;
    launchFileMock.mockReturnValue(of({ ...file }));

    await service.launchFileWithContext({
      deviceId,
      storageType,
      file,
      directoryPath: '/music',
      files: directoryFiles,
      launchMode: LaunchMode.Directory,
    });
  };

  it('should update current file favorite status when paths match', async () => {
    const file = createTestFile({ path: '/music/song1.sid', isFavorite: false });
    const directoryFiles = [file, createTestFile({ path: '/music/song2.sid', name: 'song2.sid' })];

    await launchFileForDevice(file, directoryFiles);

    service.updateCurrentFileFavoriteStatus(deviceId, file.path, true);

    const currentFile = service.getCurrentFile(deviceId)();
    expect(currentFile?.file.isFavorite).toBe(true);

    const fileContext = service.getFileContext(deviceId)();
    const updated = fileContext?.files.find((candidate) => candidate.path === file.path);
    expect(updated?.isFavorite).toBe(true);
  });

  it('should update all matching entries in file context', async () => {
    const file = createTestFile({ path: '/music/duplicate.sid' });
    const duplicateA = { ...file, name: 'duplicate-a.sid' };
    const duplicateB = { ...file, name: 'duplicate-b.sid' };
    const directoryFiles = [duplicateA, duplicateB, createTestFile({ path: '/music/other.sid' })];

    await launchFileForDevice(duplicateA, directoryFiles);

    service.updateCurrentFileFavoriteStatus(deviceId, file.path, true);

    const fileContext = service.getFileContext(deviceId)();
    const matches = fileContext?.files.filter((candidate) => candidate.path === file.path) ?? [];
    expect(matches.length).toBe(2);
    matches.forEach((entry) => expect(entry.isFavorite).toBe(true));
  });

  it('should not change state when file path does not match', async () => {
    const file = createTestFile({ path: '/music/original.sid', isFavorite: false });
    const directoryFiles = [file];

    await launchFileForDevice(file, directoryFiles);

    service.updateCurrentFileFavoriteStatus(deviceId, '/music/missing.sid', true);

    const currentFile = service.getCurrentFile(deviceId)();
    expect(currentFile?.file.isFavorite).toBe(false);

    const fileContext = service.getFileContext(deviceId)();
    expect(fileContext?.files[0].isFavorite).toBe(false);
  });

  it('should toggle favorite status off after being set to true', async () => {
    const file = createTestFile({ path: '/music/toggle.sid', isFavorite: false });
    const directoryFiles = [file];

    await launchFileForDevice(file, directoryFiles);

    service.updateCurrentFileFavoriteStatus(deviceId, file.path, true);
    service.updateCurrentFileFavoriteStatus(deviceId, file.path, false);

    const currentFile = service.getCurrentFile(deviceId)();
    expect(currentFile?.file.isFavorite).toBe(false);

    const fileContext = service.getFileContext(deviceId)();
    expect(fileContext?.files[0].isFavorite).toBe(false);
  });

  it('should not throw when device state is uninitialized', () => {
    expect(() =>
      service.updateCurrentFileFavoriteStatus('unknown-device', '/music/song.sid', true)
    ).not.toThrow();
  });

  it('should be a no-op when current file has not been launched', () => {
    service.initializePlayer(deviceId);

    service.updateCurrentFileFavoriteStatus(deviceId, '/music/song.sid', true);

    expect(service.getCurrentFile(deviceId)()).toBeNull();
    expect(service.getFileContext(deviceId)()).toBeNull();
  });

  it('should isolate updates per device', async () => {
    const otherDeviceId = 'device-favorite-test-secondary';
    const primaryFile = createTestFile({ path: '/music/primary.sid', isFavorite: false });
    const secondaryFile = createTestFile({ path: '/music/secondary.sid', isFavorite: false });

    await launchFileForDevice(primaryFile, [primaryFile]);

    service.initializePlayer(otherDeviceId);
    const launchFileMock = mockPlayerService.launchFile as ReturnType<
      typeof vi.fn<IPlayerService['launchFile']>
    >;
    launchFileMock.mockReturnValue(of({ ...secondaryFile }));
    await service.launchFileWithContext({
      deviceId: otherDeviceId,
      storageType,
      file: secondaryFile,
      directoryPath: '/music',
      files: [secondaryFile],
      launchMode: LaunchMode.Directory,
    });

    service.updateCurrentFileFavoriteStatus(deviceId, primaryFile.path, true);

    const primaryCurrent = service.getCurrentFile(deviceId)();
    const secondaryCurrent = service.getCurrentFile(otherDeviceId)();

    expect(primaryCurrent?.file.isFavorite).toBe(true);
    expect(secondaryCurrent?.file.isFavorite).toBe(false);
  });
});
