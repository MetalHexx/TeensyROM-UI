import { ChangeDetectionStrategy, Component, input, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardLayoutComponent } from '@teensyrom-nx/ui/components';
import { StorageStore } from '@teensyrom-nx/domain/storage/state';

@Component({
  selector: 'lib-directory-files',
  imports: [CommonModule, CardLayoutComponent],
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
}
