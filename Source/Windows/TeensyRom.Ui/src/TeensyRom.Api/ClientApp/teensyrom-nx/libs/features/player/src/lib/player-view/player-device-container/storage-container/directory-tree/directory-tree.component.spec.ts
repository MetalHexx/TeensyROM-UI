import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ComponentRef } from '@angular/core';
import { vi, describe, beforeEach, it, expect } from 'vitest';
import { StorageStore } from '@teensyrom-nx/application';
import { DirectoryTreeComponent, DirectoryTreeNodeType } from './directory-tree.component';
import { StorageType } from '@teensyrom-nx/domain';

interface MockStorageStore {
  getDeviceDirectories: ReturnType<typeof vi.fn>;
  navigateToDirectory: ReturnType<typeof vi.fn>;
  getDeviceStorageEntries: ReturnType<typeof vi.fn>;
  getSelectedDirectoryState: ReturnType<typeof vi.fn>;
}

interface MockMatTree {
  isExpanded: (node: unknown) => boolean;
}

describe('DirectoryTreeComponent', () => {
  let component: DirectoryTreeComponent;
  let fixture: ComponentFixture<DirectoryTreeComponent>;
  let componentRef: ComponentRef<DirectoryTreeComponent>;
  let mockStorageStore: MockStorageStore;

  beforeEach(async () => {
    mockStorageStore = {
      getDeviceDirectories: vi.fn().mockReturnValue(() => []), // Return empty array, not object
      navigateToDirectory: vi.fn(),
      getDeviceStorageEntries: vi.fn().mockReturnValue(() => ({
        'test-device-123-SD': {
          deviceId: 'test-device-123',
          storageType: StorageType.Sd,
          currentPath: '/',
          directory: null,
          isLoaded: false,
          isLoading: false,
          error: null,
          lastLoadTime: null,
        },
        'test-device-123-USB': {
          deviceId: 'test-device-123',
          storageType: StorageType.Usb,
          currentPath: '/',
          directory: null,
          isLoaded: false,
          isLoading: false,
          error: null,
          lastLoadTime: null,
        },
      })),
      getSelectedDirectoryState: vi.fn().mockReturnValue(() => null),
    };

    await TestBed.configureTestingModule({
      imports: [DirectoryTreeComponent],
      providers: [{ provide: StorageStore, useValue: mockStorageStore }],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectoryTreeComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;

    componentRef.setInput('deviceId', 'test-device-123');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set up deviceId input', () => {
    expect(component.deviceId()).toBe('test-device-123');
  });

  it('should initialize with empty directory cache', () => {
    expect(component['directoryCache']()).toEqual(new Map());
  });

  it('should build device tree structure', () => {
    const tree = component.directoryTree();
    expect(tree).toHaveLength(1);
    expect(tree[0].type).toBe(DirectoryTreeNodeType.Device);
    expect(tree[0].name).toBe('Device test-device-123');
    expect(tree[0].id).toBe('device-test-device-123');
  });

  it('should create storage type nodes for all storage types', () => {
    const tree = component.directoryTree();
    const deviceNode = tree[0];

    expect(deviceNode.children).toHaveLength(2); // SD and USB

    const storageNodes = deviceNode.children || [];
    expect(storageNodes.some((node) => node.name === 'SD Storage')).toBeTruthy();
    expect(storageNodes.some((node) => node.name === 'USB Storage')).toBeTruthy();
  });

  it('should create cache key correctly', () => {
    const key = component['createCacheKey']('device1', StorageType.Sd, '/test/path');
    expect(key).toBe('device1-SD-/test/path');
  });

  it('should call storage store on directory click for valid nodes', () => {
    const mockNode = {
      id: 'test-node',
      name: 'Test Directory',
      type: DirectoryTreeNodeType.Directory,
      icon: 'folder',
      deviceId: 'test-device',
      storageType: StorageType.Sd,
      path: '/test/path',
    };

    component.onDirectoryClick(mockNode);

    expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
      deviceId: 'test-device',
      storageType: StorageType.Sd,
      path: '/test/path',
    });
  });

  it('should not call storage store for device nodes', () => {
    const deviceNode = {
      id: 'device-test',
      name: 'Test Device',
      type: DirectoryTreeNodeType.Device,
      icon: 'smartphone',
    };

    component.onDirectoryClick(deviceNode);

    expect(mockStorageStore.navigateToDirectory).not.toHaveBeenCalled();
  });

  it('should expand storage node with directories when store has data', () => {
    // Manually populate cache to simulate what happens when StorageStore has data
    component['setCachedDirectory']('test-device-123', StorageType.Sd, '/', {
      directories: [
        { name: 'games', path: '/games' },
        { name: 'music', path: '/music' },
        { name: 'tools', path: '/tools' },
      ],
      files: [],
      path: '/',
    });

    // Get the tree after cache is populated
    const tree = component.directoryTree();

    const deviceNode = tree[0];
    const storageNodes = deviceNode.children || [];
    const sdStorageNode = storageNodes.find((node) => node.name === 'SD Storage');

    expect(sdStorageNode).toBeDefined();
    expect(sdStorageNode?.children).toHaveLength(3);
    expect(sdStorageNode?.children?.[0].name).toBe('games');
    expect(sdStorageNode?.children?.[1].name).toBe('music');
    expect(sdStorageNode?.children?.[2].name).toBe('tools');
  });

  it('should call navigateToDirectory when clicking storage node', () => {
    const storageNode = {
      id: 'test-device-123-SD',
      name: 'SD Storage',
      type: DirectoryTreeNodeType.StorageType,
      icon: 'sd_storage',
      deviceId: 'test-device-123',
      storageType: StorageType.Sd,
      path: '/',
    };

    component.onDirectoryClick(storageNode);

    expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
      deviceId: 'test-device-123',
      storageType: StorageType.Sd,
      path: '/',
    });
  });

  it('should build directory nodes from cached data', () => {
    // Manually set cache data to simulate store data being loaded
    component['setCachedDirectory']('test-device-123', StorageType.Sd, '/', {
      directories: [
        { name: 'folder1', path: '/folder1' },
        { name: 'folder2', path: '/folder2' },
      ],
      files: [],
      path: '/',
    });

    const directoryNodes = component['buildDirectoryNodes']('test-device-123', StorageType.Sd, '/');

    expect(directoryNodes).toHaveLength(2);
    expect(directoryNodes[0].name).toBe('folder1');
    expect(directoryNodes[0].path).toBe('/folder1');
    expect(directoryNodes[1].name).toBe('folder2');
    expect(directoryNodes[1].path).toBe('/folder2');
  });

  it('should return correct node id for trackBy function', () => {
    const mockNode = {
      id: 'test-node-123',
      name: 'Test Node',
      type: DirectoryTreeNodeType.Directory,
      icon: 'folder',
    };

    const result = component.trackByFn(0, mockNode);
    expect(result).toBe('test-node-123');
  });

  it('should return correct node id for expansionKey function', () => {
    const mockNode = {
      id: 'expansion-test-node',
      name: 'Expansion Test',
      type: DirectoryTreeNodeType.StorageType,
      icon: 'sd_storage',
    };

    const result = component.expansionKeyFn(mockNode);
    expect(result).toBe('expansion-test-node');
  });

  it('should trigger navigation when clicking directory node', () => {
    const mockNode = {
      id: 'test-directory-node',
      name: 'Test Directory',
      type: DirectoryTreeNodeType.Directory,
      icon: 'folder',
      deviceId: 'test-device',
      storageType: StorageType.Sd,
      path: '/test/path',
    };

    component.onDirectoryClick(mockNode);

    expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
      deviceId: 'test-device',
      storageType: StorageType.Sd,
      path: '/test/path',
    });
  });

  it('should create placeholder children for directories without cached data', () => {
    const placeholders = component['createPlaceholderChildren']('test-parent-id');

    expect(placeholders).toHaveLength(1);
    expect(placeholders[0].id).toBe('test-parent-id-placeholder');
    expect(placeholders[0].name).toBe('Loading...');
    expect(placeholders[0].type).toBe(DirectoryTreeNodeType.Placeholder);
    expect(placeholders[0].icon).toBe('hourglass_empty');
  });

  it('should identify placeholder nodes correctly', () => {
    const placeholderNode = {
      id: 'placeholder-1',
      name: 'Loading...',
      type: DirectoryTreeNodeType.Placeholder,
      icon: 'hourglass_empty',
    };

    const directoryNode = {
      id: 'dir-1',
      name: 'Directory',
      type: DirectoryTreeNodeType.Directory,
      icon: 'folder',
    };

    expect(component.isPlaceholder(0, placeholderNode)).toBeTruthy();
    expect(component.isPlaceholder(0, directoryNode)).toBeFalsy();
  });

  it('should determine children correctly based on cached data', () => {
    const mockNodeId = 'test-node-123';

    // Case 1: Has child directories - return them
    const childDirectories = [
      { id: 'child-1', name: 'Child 1', type: DirectoryTreeNodeType.Directory, icon: 'folder' },
    ];
    let result = component['determineChildren'](childDirectories, undefined, mockNodeId);
    expect(result).toEqual(childDirectories);

    // Case 2: No child directories but has cached data (empty directory) - return empty array
    const emptyCacheEntry = { directory: { directories: [], files: [], path: '/empty' } };
    result = component['determineChildren']([], emptyCacheEntry, mockNodeId);
    expect(result).toEqual([]);

    // Case 3: No child directories and no cached data (not loaded) - return placeholder
    result = component['determineChildren']([], undefined, mockNodeId);
    expect(result).toHaveLength(1);
    expect(result[0].type).toBe(DirectoryTreeNodeType.Placeholder);
  });

  it('should handle toggle clicks and trigger data loading for placeholder nodes', async () => {
    const mockNode = {
      id: 'test-storage-node',
      name: 'SD Storage',
      type: DirectoryTreeNodeType.StorageType,
      icon: 'sd_storage',
      deviceId: 'device-123',
      storageType: StorageType.Sd,
      path: '/',
      children: [
        {
          id: 'test-storage-node-placeholder',
          name: 'Loading...',
          type: DirectoryTreeNodeType.Placeholder,
          icon: 'hourglass_empty',
        },
      ],
    };

    // Mock tree component with isExpanded returning true
    const mockTreeComponent: MockMatTree = {
      isExpanded: vi.fn().mockReturnValue(true),
    };

    // Mock the tree viewChild to return our mock
    Object.defineProperty(component, 'tree', {
      value: () => mockTreeComponent,
      writable: true,
    });

    component.onToggleClick(mockNode);

    // Use setTimeout to wait for the async timeout in onToggleClick
    await new Promise((resolve) => setTimeout(resolve, 100));

    expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
      deviceId: 'device-123',
      storageType: StorageType.Sd,
      path: '/',
    });
  });

  it('should not trigger data loading when node is collapsed', async () => {
    const mockNode = {
      id: 'test-storage-node',
      name: 'SD Storage',
      type: DirectoryTreeNodeType.StorageType,
      icon: 'sd_storage',
      deviceId: 'device-123',
      storageType: StorageType.Sd,
      path: '/',
      children: [
        {
          id: 'test-storage-node-placeholder',
          name: 'Loading...',
          type: DirectoryTreeNodeType.Placeholder,
          icon: 'hourglass_empty',
        },
      ],
    };

    // Mock tree component with isExpanded returning false (collapsed)
    const mockTreeComponent: MockMatTree = {
      isExpanded: vi.fn().mockReturnValue(false),
    };

    Object.defineProperty(component, 'tree', {
      value: () => mockTreeComponent,
      writable: true,
    });

    component.onToggleClick(mockNode);

    await new Promise((resolve) => setTimeout(resolve, 100));

    expect(mockStorageStore.navigateToDirectory).not.toHaveBeenCalled();
  });

  it('should not trigger data loading for nodes without placeholders', async () => {
    const mockNode = {
      id: 'test-storage-node',
      name: 'SD Storage',
      type: DirectoryTreeNodeType.StorageType,
      icon: 'sd_storage',
      deviceId: 'device-123',
      storageType: StorageType.Sd,
      path: '/',
      children: [
        {
          id: 'test-real-directory',
          name: 'Games',
          type: DirectoryTreeNodeType.Directory,
          icon: 'folder',
        },
      ],
    };

    // Mock tree component with isExpanded returning true
    const mockTreeComponent: MockMatTree = {
      isExpanded: vi.fn().mockReturnValue(true),
    };

    Object.defineProperty(component, 'tree', {
      value: () => mockTreeComponent,
      writable: true,
    });

    component.onToggleClick(mockNode);

    await new Promise((resolve) => setTimeout(resolve, 100));

    expect(mockStorageStore.navigateToDirectory).not.toHaveBeenCalled();
  });

  it('should auto-expand device node after view init', async () => {
    const mockTreeComponent = {
      isExpanded: vi.fn().mockReturnValue(true),
      expand: vi.fn(),
    };

    Object.defineProperty(component, 'tree', {
      value: () => mockTreeComponent,
      writable: true,
    });

    component.ngAfterViewInit();

    await new Promise((resolve) => setTimeout(resolve, 100));

    const tree = component.directoryTree();
    const deviceNode = tree[0];

    expect(mockTreeComponent.expand).toHaveBeenCalledWith(deviceNode);
    expect(deviceNode.type).toBe(DirectoryTreeNodeType.Device);
  });

  it('should not auto-expand storage nodes when multiple exist', async () => {
    const mockTreeComponent = {
      isExpanded: vi.fn().mockReturnValue(true),
      expand: vi.fn(),
    };

    Object.defineProperty(component, 'tree', {
      value: () => mockTreeComponent,
      writable: true,
    });

    component.ngAfterViewInit();

    await new Promise((resolve) => setTimeout(resolve, 100));

    const tree = component.directoryTree();
    const deviceNode = tree[0];

    expect(mockTreeComponent.expand).toHaveBeenCalledWith(deviceNode);
    expect(deviceNode.children).toHaveLength(2); // SD and USB

    // Should not expand storage nodes when there are multiple
    const storageNodes = deviceNode.children?.filter(
      (child) => child.type === DirectoryTreeNodeType.StorageType
    );
    storageNodes?.forEach((storageNode) => {
      expect(mockTreeComponent.expand).not.toHaveBeenCalledWith(storageNode);
    });
  });
});

// Separate test suite for single storage scenario
describe('DirectoryTreeComponent - Single Storage', () => {
  let component: DirectoryTreeComponent;
  let fixture: ComponentFixture<DirectoryTreeComponent>;
  let componentRef: ComponentRef<DirectoryTreeComponent>;
  let mockStorageStore: MockStorageStore;

  beforeEach(async () => {
    mockStorageStore = {
      getDeviceDirectories: vi.fn().mockReturnValue(() => []),
      navigateToDirectory: vi.fn(),
      getDeviceStorageEntries: vi.fn().mockReturnValue(() => ({
        'test-device-123-SD': {
          deviceId: 'test-device-123',
          storageType: StorageType.Sd,
          currentPath: '/',
          directory: null,
          isLoaded: false,
          isLoading: false,
          error: null,
          lastLoadTime: null,
        },
      })),
      getSelectedDirectoryState: vi.fn().mockReturnValue(() => null),
    };

    await TestBed.configureTestingModule({
      imports: [DirectoryTreeComponent],
      providers: [{ provide: StorageStore, useValue: mockStorageStore }],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectoryTreeComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;

    componentRef.setInput('deviceId', 'test-device-123');
    fixture.detectChanges();
  });

  it('should auto-expand single storage node when only one exists', async () => {
    const mockTreeComponent = {
      isExpanded: vi.fn().mockReturnValue(true),
      expand: vi.fn(),
    };

    Object.defineProperty(component, 'tree', {
      value: () => mockTreeComponent,
      writable: true,
    });

    component.ngAfterViewInit();

    await new Promise((resolve) => setTimeout(resolve, 100));

    const tree = component.directoryTree();
    const deviceNode = tree[0];
    const storageNode = deviceNode.children?.[0];

    expect(mockTreeComponent.expand).toHaveBeenCalledWith(deviceNode);
    expect(mockTreeComponent.expand).toHaveBeenCalledWith(storageNode);
    expect(mockTreeComponent.expand).toHaveBeenCalledTimes(2);
    expect(deviceNode.children).toHaveLength(1);
  });
});
