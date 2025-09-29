import { vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { PlayerToolbarComponent } from './player-toolbar.component';
import { PLAYER_CONTEXT, IPlayerContext } from '@teensyrom-nx/application';
import { LaunchMode } from '@teensyrom-nx/domain';

describe('PlayerToolbarComponent', () => {
  let component: PlayerToolbarComponent;
  let fixture: ComponentFixture<PlayerToolbarComponent>;

  const mockPlayerContext: Partial<IPlayerContext> = {
    launchRandomFile: vi.fn(),
    toggleShuffleMode: vi.fn(),
    getLaunchMode: vi.fn().mockReturnValue(signal(LaunchMode.Directory).asReadonly()),
    isLoading: vi.fn().mockReturnValue(signal(false).asReadonly()),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlayerToolbarComponent],
      providers: [
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
});
