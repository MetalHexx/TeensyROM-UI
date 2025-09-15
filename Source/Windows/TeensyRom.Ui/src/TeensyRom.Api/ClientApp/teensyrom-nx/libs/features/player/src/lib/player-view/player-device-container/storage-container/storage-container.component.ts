import { Component, input, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DirectoryTreeComponent } from './directory-tree/directory-tree.component';
import { DirectoryFilesComponent } from './directory-files/directory-files.component';
import { SearchToolbarComponent } from './search-toolbar/search-toolbar.component';
import { StorageStore } from '@teensyrom-nx/domain/storage/state';

@Component({
  selector: 'lib-storage-container',
  imports: [CommonModule, DirectoryTreeComponent, DirectoryFilesComponent, SearchToolbarComponent],
  templateUrl: './storage-container.component.html',
  styleUrl: './storage-container.component.scss',
})
export class StorageContainerComponent {
  deviceId = input.required<string>();

  readonly storageStore = inject(StorageStore);

  readonly deviceEntries = computed(() =>
    this.storageStore['getDeviceStorageEntries'](this.deviceId())()
  );
}
