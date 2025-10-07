import { Component, input, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DirectoryTreeComponent } from './directory-tree/directory-tree.component';
import { DirectoryFilesComponent } from './directory-files/directory-files.component';
import { SearchToolbarComponent } from './search-toolbar/search-toolbar.component';
import { FilterToolbarComponent } from './filter-toolbar/filter-toolbar.component';
import { StorageStore } from '@teensyrom-nx/application';
import { DirectoryTrailComponent } from './directory-trail/directory-trail.component';
import { SearchResultsComponent } from './search-results/search-results.component';

@Component({
  selector: 'lib-storage-container',
  imports: [
    CommonModule,
    DirectoryTreeComponent,
    DirectoryFilesComponent,
    SearchToolbarComponent,
    FilterToolbarComponent,
    DirectoryTrailComponent,
    SearchResultsComponent,
  ],
  templateUrl: './storage-container.component.html',
  styleUrl: './storage-container.component.scss',
})
export class StorageContainerComponent {
  deviceId = input.required<string>();

  readonly storageStore = inject(StorageStore);

  readonly deviceEntries = computed(() =>
    this.storageStore.getDeviceStorageEntries(this.deviceId())()
  );

  readonly hasActiveSearch = computed(() => {
    const selectedDir = this.storageStore.getSelectedDirectoryState(this.deviceId())();
    if (!selectedDir) {
      return false;
    }
    const searchState = this.storageStore.getSearchState(this.deviceId(), selectedDir.storageType)();
    return searchState?.hasSearched ?? false;
  });
}
