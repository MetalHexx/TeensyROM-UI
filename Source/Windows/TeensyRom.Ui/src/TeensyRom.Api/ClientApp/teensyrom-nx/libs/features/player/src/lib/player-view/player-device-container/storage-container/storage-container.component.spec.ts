import { vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { StorageContainerComponent } from './storage-container.component';
import {
  STORAGE_SERVICE,
  IStorageService,
  StorageDirectory,
  PlayerStatus,
} from '@teensyrom-nx/domain';
import { PLAYER_CONTEXT, IPlayerContext } from '@teensyrom-nx/application';
import { of } from 'rxjs';

describe('StorageContainerComponent', () => {
  let component: StorageContainerComponent;
  let fixture: ComponentFixture<StorageContainerComponent>;

  const mockStorageService: Partial<IStorageService> = {
    getDirectory: () => of({} as StorageDirectory),
  };

  const mockPlayerContext: Partial<IPlayerContext> = {
    initializePlayer: vi.fn(),
    removePlayer: vi.fn(),
    launchFileWithContext: vi.fn(),
    getCurrentFile: vi.fn().mockReturnValue(signal(null).asReadonly()),
    getFileContext: vi.fn().mockReturnValue(signal(null).asReadonly()),
    isLoading: vi.fn().mockReturnValue(signal(false).asReadonly()),
    getError: vi.fn().mockReturnValue(signal(null).asReadonly()),
    getStatus: vi.fn().mockReturnValue(signal(PlayerStatus.Stopped).asReadonly()),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StorageContainerComponent],
      providers: [
        { provide: STORAGE_SERVICE, useValue: mockStorageService },
        { provide: PLAYER_CONTEXT, useValue: mockPlayerContext },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(StorageContainerComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

