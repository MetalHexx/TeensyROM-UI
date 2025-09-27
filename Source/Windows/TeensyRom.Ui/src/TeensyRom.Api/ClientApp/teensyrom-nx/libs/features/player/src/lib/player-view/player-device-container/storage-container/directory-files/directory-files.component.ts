import { ChangeDetectionStrategy, Component, input, inject, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardLayoutComponent } from '@teensyrom-nx/ui/components';
import { StorageStore } from '@teensyrom-nx/application';
import { DirectoryItem, FileItem } from '@teensyrom-nx/domain';
import { DirectoryItemComponent } from './directory-item/directory-item.component';
import { FileItemComponent } from './file-item/file-item.component';

@Component({
  selector: 'lib-directory-files',
  imports: [CommonModule, CardLayoutComponent, DirectoryItemComponent, FileItemComponent],
  templateUrl: './directory-files.component.html',
  styleUrl: './directory-files.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DirectoryFilesComponent {
  deviceId = input.required<string>();

  private readonly storageStore = inject(StorageStore);

  readonly selectedDirectoryState = computed(() =>
    this.storageStore.getSelectedDirectoryState(this.deviceId())()
  );

  readonly directoryContents = computed(() => {
    const state = this.selectedDirectoryState();
    if (!state?.directory) {
      return { files: [], directories: [], hasContent: false };
    }

    return {
      files: state.directory.files || [],
      directories: state.directory.directories || [],
      hasContent: true,
      currentPath: state.currentPath,
      storageType: state.storageType,
      deviceId: state.deviceId,
      isLoading: state.isLoading,
      error: state.error,
    };
  });

  readonly selectedItem = signal<DirectoryItem | FileItem | null>(null);

  readonly combinedItems = computed(() => {
    const contents = this.directoryContents();
    const directories = contents.directories.map((dir) => ({
      ...dir,
      itemType: 'directory' as const,
    }));
    const files = contents.files.map((file) => ({
      ...file,
      itemType: 'file' as const,
    }));
    return [...directories, ...files];
  });

  isDirectory(item: DirectoryItem | FileItem): item is DirectoryItem {
    return 'itemType' in item && (item as { itemType: string }).itemType === 'directory';
  }

  isSelected(item: DirectoryItem | FileItem): boolean {
    const selected = this.selectedItem();
    return selected !== null && selected.path === item.path;
  }

  onItemSelected(item: DirectoryItem | FileItem): void {
    this.selectedItem.set(item);
  }

  onDirectoryDoubleClick(directory: DirectoryItem): void {
    const contents = this.directoryContents();
    if (contents.storageType && contents.deviceId) {
      this.storageStore.navigateToDirectory({
        deviceId: contents.deviceId,
        storageType: contents.storageType,
        path: directory.path,
      });
      this.selectedItem.set(null);
    }
  }
}
