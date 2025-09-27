import { ComponentFixture, TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { signal } from '@angular/core';
import { DirectoryFilesComponent } from './directory-files.component';
import {
  DirectoryItem,
  FileItem,
  FileItemType,
  STORAGE_SERVICE,
  IStorageService,
  StorageDirectory,
} from '@teensyrom-nx/domain';
import { StorageStore } from '@teensyrom-nx/application';
import { of } from 'rxjs';

describe('DirectoryFilesComponent', () => {
  let component: DirectoryFilesComponent;
  let fixture: ComponentFixture<DirectoryFilesComponent>;
  let mockStorageStore: Partial<StorageStore>;

  const mockDirectoryItem: DirectoryItem = {
    name: 'Test Folder',
    path: '/test/folder',
  };

  const mockFileItem: FileItem = {
    name: 'test-file.sid',
    path: '/test/file.sid',
    size: 1024,
    type: FileItemType.Song,
    isFavorite: false,
    title: '',
    creator: '',
    releaseInfo: '',
    description: '',
    shareUrl: '',
    metadataSource: '',
    meta1: '',
    meta2: '',
    metadataSourcePath: '',
    parentPath: '',
    playLength: '',
    subtuneLengths: [],
    startSubtuneNum: 0,
    images: [],
  };

  const mockStorageService: Partial<IStorageService> = {
    getDirectories: () => of([]),
    getDirectory: () => of({} as StorageDirectory),
  };

  beforeEach(async () => {
    mockStorageStore = {
      getSelectedDirectoryState: vi.fn().mockReturnValue(
        signal({
          directory: {
            directories: [mockDirectoryItem],
            files: [mockFileItem],
            path: '/test',
          },
          currentPath: '/test',
          storageType: 'SD',
          deviceId: 'device-1',
          isLoading: false,
          isLoaded: true,
          error: null,
        })
      ),
      navigateToDirectory: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [DirectoryFilesComponent],
      providers: [
        { provide: STORAGE_SERVICE, useValue: mockStorageService },
        { provide: StorageStore, useValue: mockStorageStore },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectoryFilesComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'device-1');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should combine directories and files into single data source', () => {
    const combined = component.combinedItems();
    expect(combined).toHaveLength(2);
    expect(combined[0]).toHaveProperty('itemType', 'directory');
    expect(combined[1]).toHaveProperty('itemType', 'file');
  });

  it('should correctly identify directories with type guard', () => {
    const combined = component.combinedItems();
    expect(component.isDirectory(combined[0])).toBe(true);
    expect(component.isDirectory(combined[1])).toBe(false);
  });

  it('should update selection when item clicked', () => {
    const combined = component.combinedItems();
    component.onItemSelected(combined[0]);
    expect(component.selectedItem()).toEqual(combined[0]);
  });

  it('should call navigateToDirectory on directory double-click', () => {
    const combined = component.combinedItems();
    const directoryItem = combined[0] as DirectoryItem;

    component.onDirectoryDoubleClick(directoryItem);

    expect(mockStorageStore.navigateToDirectory).toHaveBeenCalledWith({
      deviceId: 'device-1',
      storageType: 'SD',
      path: directoryItem.path,
    });
  });

  it('should clear selection when directory changes', () => {
    const combined = component.combinedItems();
    component.onItemSelected(combined[0]);
    expect(component.selectedItem()).toBeTruthy();

    component.onDirectoryDoubleClick(combined[0] as DirectoryItem);
    expect(component.selectedItem()).toBe(null);
  });

  it('should render correct number of list items', () => {
    const items = fixture.nativeElement.querySelectorAll('.file-list-item');
    expect(items.length).toBe(2);
  });

  it('should determine if item is selected correctly', () => {
    const combined = component.combinedItems();
    component.onItemSelected(combined[0]);

    expect(component.isSelected(combined[0])).toBe(true);
    expect(component.isSelected(combined[1])).toBe(false);
  });
});
