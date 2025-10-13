import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PlayerToolbarActionsComponent } from './player-toolbar-actions.component';
import { PLAYER_CONTEXT } from '@teensyrom-nx/application';
import { signal } from '@angular/core';
import { LaunchMode } from '@teensyrom-nx/domain';

describe('PlayerToolbarActionsComponent', () => {
  let component: PlayerToolbarActionsComponent;
  let fixture: ComponentFixture<PlayerToolbarActionsComponent>;
  let mockPlayerContext: {
    toggleShuffleMode: ReturnType<typeof vi.fn>;
    getLaunchMode: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    mockPlayerContext = {
      toggleShuffleMode: vi.fn(),
      getLaunchMode: vi.fn(() => signal(LaunchMode.Single)),
    };

    await TestBed.configureTestingModule({
      imports: [PlayerToolbarActionsComponent],
      providers: [
        { provide: PLAYER_CONTEXT, useValue: mockPlayerContext }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PlayerToolbarActionsComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'test-device');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle shuffle mode when button clicked', () => {
    component.toggleShuffleMode();
    expect(mockPlayerContext.toggleShuffleMode).toHaveBeenCalledWith('test-device');
  });

  it('should detect shuffle mode correctly', () => {
    mockPlayerContext.getLaunchMode = vi.fn(() => signal(LaunchMode.Shuffle));
    expect(component.isShuffleMode()).toBe(true);

    mockPlayerContext.getLaunchMode = vi.fn(() => signal(LaunchMode.Single));
    expect(component.isShuffleMode()).toBe(false);
  });
});
