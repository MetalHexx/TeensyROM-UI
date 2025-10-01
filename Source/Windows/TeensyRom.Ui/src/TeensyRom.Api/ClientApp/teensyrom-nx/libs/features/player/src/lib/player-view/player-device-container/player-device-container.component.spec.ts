import { vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { PlayerDeviceContainerComponent } from './player-device-container.component';
import { PLAYER_CONTEXT, IPlayerContext } from '@teensyrom-nx/application';
import { LaunchMode, PlayerStatus } from '@teensyrom-nx/domain';

describe('PlayerDeviceContainerComponent', () => {
  let component: PlayerDeviceContainerComponent;
  let fixture: ComponentFixture<PlayerDeviceContainerComponent>;
  let mockPlayerContext: IPlayerContext;

  beforeEach(async () => {
    // Create a mock player context
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
      getShuffleSettings: vi.fn().mockReturnValue(signal(null).asReadonly()),
    } satisfies IPlayerContext;

    await TestBed.configureTestingModule({
      imports: [PlayerDeviceContainerComponent],
      providers: [
        { provide: PLAYER_CONTEXT, useValue: mockPlayerContext },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PlayerDeviceContainerComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('device', {
      id: 'test-device',
      name: 'Test Device',
      status: 'connected'
    });
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
