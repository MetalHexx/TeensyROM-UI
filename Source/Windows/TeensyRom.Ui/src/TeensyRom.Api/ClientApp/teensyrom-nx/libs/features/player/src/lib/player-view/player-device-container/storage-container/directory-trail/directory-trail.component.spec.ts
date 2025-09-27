import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { StorageStore } from '@teensyrom-nx/application';
import { CompactCardLayoutComponent } from '@teensyrom-nx/ui/components';
import { DirectoryTrailComponent } from './directory-trail.component';
import { DirectoryNavigateComponent } from './directory-navigate/directory-navigate.component';
import { DirectoryBreadcrumbComponent } from './directory-breadcrumb/directory-breadcrumb.component';

describe('DirectoryTrailComponent', () => {
  let component: DirectoryTrailComponent;
  let fixture: ComponentFixture<DirectoryTrailComponent>;

  describe('Basic Functionality', () => {
    let mockStorageStore: any;

    beforeEach(async () => {
      // Create mock storage store with all methods needed
      mockStorageStore = {
        getSelectedDirectoryState: () => () => ({
          currentPath: '/games/arcade',
          isLoading: false,
          deviceId: 'test-device',
          storageType: 'SD',
          directory: null,
          isLoaded: true,
          error: null,
          lastLoadTime: Date.now(),
        }),
        getSelectedDirectoryForDevice: () => ({
          deviceId: 'test-device',
          storageType: 'SD',
          path: '/games/arcade',
        }),
        navigationHistory: () => ({
          'test-device': {
            history: ['/', '/games', '/games/arcade'],
            currentIndex: 2,
            maxHistorySize: 50,
          },
        }),
        navigateUpOneDirectory: vi.fn(),
        refreshDirectory: vi.fn(),
        navigateToDirectory: vi.fn(),
        navigateDirectoryBackward: vi.fn(),
        navigateDirectoryForward: vi.fn(),
      };

      await TestBed.configureTestingModule({
        imports: [
          DirectoryTrailComponent,
          CompactCardLayoutComponent,
          DirectoryNavigateComponent,
          DirectoryBreadcrumbComponent,
        ],
        providers: [{ provide: StorageStore, useValue: mockStorageStore }],
      }).compileComponents();

      fixture = TestBed.createComponent(DirectoryTrailComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();
    });

    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should require deviceId input', () => {
      expect(component.deviceId()).toBe('test-device');
    });

    it('should get selected directory state from store', () => {
      const state = component.selectedDirectoryState();

      expect(state).toEqual({
        currentPath: '/games/arcade',
        isLoading: false,
        deviceId: 'test-device',
        storageType: 'SD',
        directory: null,
        isLoaded: true,
        error: null,
        lastLoadTime: expect.any(Number),
      });
    });

    it('should compute current path from state', () => {
      expect(component.currentPath()).toBe('/games/arcade');
    });

    it('should compute storage type label for SD card', () => {
      expect(component.storageTypeLabel()).toBe('SD Card');
    });

    it('should compute canNavigateUp based on current path', () => {
      expect(component.canNavigateUp()).toBe(true);
    });

    it('should compute loading state from store', () => {
      expect(component.isLoading()).toBe(false);
    });

    it('should compute canNavigateBack based on navigation history', () => {
      expect(component.canNavigateBack()).toBe(true); // currentIndex = 2, can go back
    });

    it('should compute canNavigateForward based on navigation history', () => {
      expect(component.canNavigateForward()).toBe(false); // currentIndex = 2, at end of history
    });

    it('should call store navigateUpOneDirectory on up click', () => {
      component.onUpClick();

      expect(mockStorageStore.navigateUpOneDirectory).toHaveBeenCalledWith({
        deviceId: 'test-device',
        storageType: 'SD',
      });
    });

    it('should call store refreshDirectory on refresh click', () => {
      component.onRefreshClick();

      expect(mockStorageStore.refreshDirectory).toHaveBeenCalledWith({
        deviceId: 'test-device',
        storageType: 'SD',
      });
    });

    it('should call store navigateToDirectory on navigation request', () => {
      component.onNavigationRequested('/games');

      expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
        deviceId: 'test-device',
        storageType: 'SD',
        path: '/games',
      });
    });

    it('should call store navigateDirectoryBackward on back click when can navigate back', () => {
      component.onBackClick();

      expect(mockStorageStore.navigateDirectoryBackward).toHaveBeenCalledWith({
        deviceId: 'test-device',
      });
    });
  });

  describe('Forward Navigation', () => {
    let mockStorageStore: any;

    beforeEach(async () => {
      // Create mock storage store with forward navigation enabled
      mockStorageStore = {
        getSelectedDirectoryState: () => () => ({
          currentPath: '/games',
          isLoading: false,
          deviceId: 'test-device',
          storageType: 'SD',
          directory: null,
          isLoaded: true,
          error: null,
          lastLoadTime: Date.now(),
        }),
        getSelectedDirectoryForDevice: () => ({
          deviceId: 'test-device',
          storageType: 'SD',
          path: '/games',
        }),
        navigationHistory: () => ({
          'test-device': {
            history: ['/', '/games', '/games/arcade'],
            currentIndex: 1, // Can go forward to /games/arcade
            maxHistorySize: 50,
          },
        }),
        navigateUpOneDirectory: vi.fn(),
        refreshDirectory: vi.fn(),
        navigateToDirectory: vi.fn(),
        navigateDirectoryBackward: vi.fn(),
        navigateDirectoryForward: vi.fn(),
      };

      await TestBed.configureTestingModule({
        imports: [
          DirectoryTrailComponent,
          CompactCardLayoutComponent,
          DirectoryNavigateComponent,
          DirectoryBreadcrumbComponent,
        ],
        providers: [{ provide: StorageStore, useValue: mockStorageStore }],
      }).compileComponents();

      fixture = TestBed.createComponent(DirectoryTrailComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();
    });

    it('should call store navigateDirectoryForward on forward click when can navigate forward', () => {
      component.onForwardClick();

      expect(mockStorageStore.navigateDirectoryForward).toHaveBeenCalledWith({
        deviceId: 'test-device',
      });
    });

    it('should compute canNavigateForward correctly when in middle of history', () => {
      expect(component.canNavigateForward()).toBe(true);
    });
  });

  describe('Null State Handling', () => {
    let mockStorageStore: any;

    beforeEach(async () => {
      mockStorageStore = {
        getSelectedDirectoryState: () => () => null,
        getSelectedDirectoryForDevice: () => null,
        navigationHistory: () => ({}),
        navigateUpOneDirectory: vi.fn(),
        refreshDirectory: vi.fn(),
        navigateToDirectory: vi.fn(),
        navigateDirectoryBackward: vi.fn(),
        navigateDirectoryForward: vi.fn(),
      };

      await TestBed.configureTestingModule({
        imports: [
          DirectoryTrailComponent,
          CompactCardLayoutComponent,
          DirectoryNavigateComponent,
          DirectoryBreadcrumbComponent,
        ],
        providers: [{ provide: StorageStore, useValue: mockStorageStore }],
      }).compileComponents();

      fixture = TestBed.createComponent(DirectoryTrailComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();
    });

    it('should default to root path when state is null', () => {
      expect(component.currentPath()).toBe('/');
    });

    it('should default storage type label when no selection', () => {
      expect(component.storageTypeLabel()).toBe('Storage');
    });

    it('should handle loading state when no directory state', () => {
      expect(component.isLoading()).toBe(false);
    });

    it('should not call store when no selected directory on up click', () => {
      component.onUpClick();

      expect(mockStorageStore.navigateUpOneDirectory).not.toHaveBeenCalled();
    });

    it('should not call store when no selected directory on refresh click', () => {
      component.onRefreshClick();

      expect(mockStorageStore.refreshDirectory).not.toHaveBeenCalled();
    });

    it('should not call store when no selected directory on navigation request', () => {
      component.onNavigationRequested('/games');

      expect(mockStorageStore.navigateToDirectory).not.toHaveBeenCalled();
    });

    it('should return false for canNavigateBack when no navigation history', () => {
      expect(component.canNavigateBack()).toBe(false);
    });

    it('should return false for canNavigateForward when no navigation history', () => {
      expect(component.canNavigateForward()).toBe(false);
    });

    it('should not call store navigateDirectoryBackward when cannot navigate back', () => {
      component.onBackClick();

      expect(mockStorageStore.navigateDirectoryBackward).not.toHaveBeenCalled();
    });

    it('should not call store navigateDirectoryForward when cannot navigate forward', () => {
      component.onForwardClick();

      expect(mockStorageStore.navigateDirectoryForward).not.toHaveBeenCalled();
    });
  });

  describe('USB Storage Type', () => {
    let mockStorageStore: any;

    beforeEach(async () => {
      mockStorageStore = {
        getSelectedDirectoryState: () => () => ({
          currentPath: '/files',
          isLoading: false,
          deviceId: 'test-device',
          storageType: 'USB',
          directory: null,
          isLoaded: true,
          error: null,
          lastLoadTime: Date.now(),
        }),
        getSelectedDirectoryForDevice: () => ({
          deviceId: 'test-device',
          storageType: 'USB',
          path: '/files',
        }),
        navigationHistory: () => ({
          'test-device': { history: ['/files'], currentIndex: 0, maxHistorySize: 50 },
        }),
        navigateUpOneDirectory: vi.fn(),
        refreshDirectory: vi.fn(),
        navigateToDirectory: vi.fn(),
        navigateDirectoryBackward: vi.fn(),
        navigateDirectoryForward: vi.fn(),
      };

      await TestBed.configureTestingModule({
        imports: [
          DirectoryTrailComponent,
          CompactCardLayoutComponent,
          DirectoryNavigateComponent,
          DirectoryBreadcrumbComponent,
        ],
        providers: [{ provide: StorageStore, useValue: mockStorageStore }],
      }).compileComponents();

      fixture = TestBed.createComponent(DirectoryTrailComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();
    });

    it('should compute storage type label for USB', () => {
      expect(component.storageTypeLabel()).toBe('USB Drive');
    });
  });

  describe('Root Path Navigation', () => {
    let mockStorageStore: any;

    beforeEach(async () => {
      mockStorageStore = {
        getSelectedDirectoryState: () => () => ({
          currentPath: '/',
          isLoading: false,
          deviceId: 'test-device',
          storageType: 'SD',
          directory: null,
          isLoaded: true,
          error: null,
          lastLoadTime: Date.now(),
        }),
        getSelectedDirectoryForDevice: () => ({
          deviceId: 'test-device',
          storageType: 'SD',
          path: '/',
        }),
        navigationHistory: () => ({
          'test-device': { history: ['/'], currentIndex: 0, maxHistorySize: 50 },
        }),
        navigateUpOneDirectory: vi.fn(),
        refreshDirectory: vi.fn(),
        navigateToDirectory: vi.fn(),
        navigateDirectoryBackward: vi.fn(),
        navigateDirectoryForward: vi.fn(),
      };

      await TestBed.configureTestingModule({
        imports: [
          DirectoryTrailComponent,
          CompactCardLayoutComponent,
          DirectoryNavigateComponent,
          DirectoryBreadcrumbComponent,
        ],
        providers: [{ provide: StorageStore, useValue: mockStorageStore }],
      }).compileComponents();

      fixture = TestBed.createComponent(DirectoryTrailComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();
    });

    it('should not allow navigate up from root path', () => {
      expect(component.canNavigateUp()).toBe(false);
    });
  });

  describe('Loading State', () => {
    let mockStorageStore: any;

    beforeEach(async () => {
      mockStorageStore = {
        getSelectedDirectoryState: () => () => ({
          currentPath: '/games',
          isLoading: true,
          deviceId: 'test-device',
          storageType: 'SD',
          directory: null,
          isLoaded: false,
          error: null,
          lastLoadTime: null,
        }),
        getSelectedDirectoryForDevice: () => ({
          deviceId: 'test-device',
          storageType: 'SD',
          path: '/games',
        }),
        navigationHistory: () => ({
          'test-device': { history: ['/games'], currentIndex: 0, maxHistorySize: 50 },
        }),
        navigateUpOneDirectory: vi.fn(),
        refreshDirectory: vi.fn(),
        navigateToDirectory: vi.fn(),
        navigateDirectoryBackward: vi.fn(),
        navigateDirectoryForward: vi.fn(),
      };

      await TestBed.configureTestingModule({
        imports: [
          DirectoryTrailComponent,
          CompactCardLayoutComponent,
          DirectoryNavigateComponent,
          DirectoryBreadcrumbComponent,
        ],
        providers: [{ provide: StorageStore, useValue: mockStorageStore }],
      }).compileComponents();

      fixture = TestBed.createComponent(DirectoryTrailComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();
    });

    it('should reflect loading state in computed properties', () => {
      expect(component.isLoading()).toBe(true);
    });

    it('should pass loading state to child components', () => {
      const navigateComponent = fixture.debugElement.query(
        (el) => el.componentInstance instanceof DirectoryNavigateComponent
      )?.componentInstance;

      expect(navigateComponent?.isLoading()).toBe(true);
    });
  });

  describe('Child Component Integration', () => {
    let mockStorageStore: any;

    beforeEach(async () => {
      mockStorageStore = {
        getSelectedDirectoryState: () => () => ({
          currentPath: '/games/arcade',
          isLoading: false,
          deviceId: 'test-device',
          storageType: 'SD',
          directory: null,
          isLoaded: true,
          error: null,
          lastLoadTime: Date.now(),
        }),
        getSelectedDirectoryForDevice: () => ({
          deviceId: 'test-device',
          storageType: 'SD',
          path: '/games/arcade',
        }),
        navigationHistory: () => ({
          'test-device': {
            history: ['/', '/games', '/games/arcade'],
            currentIndex: 2,
            maxHistorySize: 50,
          },
        }),
        navigateUpOneDirectory: vi.fn(),
        refreshDirectory: vi.fn(),
        navigateToDirectory: vi.fn(),
        navigateDirectoryBackward: vi.fn(),
        navigateDirectoryForward: vi.fn(),
      };

      await TestBed.configureTestingModule({
        imports: [
          DirectoryTrailComponent,
          CompactCardLayoutComponent,
          DirectoryNavigateComponent,
          DirectoryBreadcrumbComponent,
        ],
        providers: [{ provide: StorageStore, useValue: mockStorageStore }],
      }).compileComponents();

      fixture = TestBed.createComponent(DirectoryTrailComponent);
      component = fixture.componentInstance;
      fixture.componentRef.setInput('deviceId', 'test-device');
      fixture.detectChanges();
    });

    it('should pass correct props to directory navigate component', () => {
      const navigateComponent = fixture.debugElement.query(
        (el) => el.componentInstance instanceof DirectoryNavigateComponent
      )?.componentInstance;

      expect(navigateComponent?.canNavigateUp()).toBe(true);
      expect(navigateComponent?.canNavigateBack()).toBe(true);
      expect(navigateComponent?.canNavigateForward()).toBe(false);
      expect(navigateComponent?.isLoading()).toBe(false);
    });

    it('should pass correct props to directory breadcrumb component', () => {
      const breadcrumbComponent = fixture.debugElement.query(
        (el) => el.componentInstance instanceof DirectoryBreadcrumbComponent
      )?.componentInstance;

      expect(breadcrumbComponent?.currentPath()).toBe('/games/arcade');
      expect(breadcrumbComponent?.storageType()).toBe('SD Card');
    });
  });
});
