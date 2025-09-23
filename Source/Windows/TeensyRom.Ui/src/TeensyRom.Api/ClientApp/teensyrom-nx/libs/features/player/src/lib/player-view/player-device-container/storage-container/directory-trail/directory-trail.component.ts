import { Component, input, inject, computed } from '@angular/core';
import { StorageStore } from '@teensyrom-nx/domain/storage/state';
import { CompactCardLayoutComponent } from '@teensyrom-nx/ui/components';
import { DirectoryNavigateComponent } from './directory-navigate/directory-navigate.component';
import { DirectoryBreadcrumbComponent } from './directory-breadcrumb/directory-breadcrumb.component';

@Component({
  selector: 'lib-directory-trail',
  standalone: true,
  imports: [CompactCardLayoutComponent, DirectoryNavigateComponent, DirectoryBreadcrumbComponent],
  templateUrl: './directory-trail.component.html',
  styleUrl: './directory-trail.component.scss',
})
export class DirectoryTrailComponent {
  // Inputs
  deviceId = input.required<string>();

  // Store injection
  private readonly storageStore = inject(StorageStore);

  // Computed state selectors
  selectedDirectoryState = computed(() => {
    const deviceId = this.deviceId();
    return this.storageStore.getSelectedDirectoryState(deviceId)();
  });

  selectedDirectory = computed(() => {
    const deviceId = this.deviceId();
    return this.storageStore.getSelectedDirectoryForDevice(deviceId);
  });

  // Computed properties for child components
  currentPath = computed(() => {
    const state = this.selectedDirectoryState();
    return state?.currentPath || '/';
  });

  storageTypeLabel = computed(() => {
    const selected = this.selectedDirectory();
    if (!selected) return 'Storage';

    switch (selected.storageType) {
      case 'SD':
        return 'SD Card';
      case 'USB':
        return 'USB Drive';
      default:
        return 'Storage';
    }
  });

  canNavigateUp = computed(() => {
    const path = this.currentPath();
    return path !== '/' && path !== '';
  });

  isLoading = computed(() => {
    const state = this.selectedDirectoryState();
    return state?.isLoading ?? false;
  });

  // Event handlers
  onBackClick(): void {
    // Back functionality not implemented yet
    console.log('Back clicked - not implemented');
  }

  onForwardClick(): void {
    // Forward functionality not implemented yet
    console.log('Forward clicked - not implemented');
  }

  onUpClick(): void {
    const selected = this.selectedDirectory();
    if (!selected) return;

    this.storageStore.navigateUpOneDirectory({
      deviceId: selected.deviceId,
      storageType: selected.storageType,
    });
  }

  onRefreshClick(): void {
    const selected = this.selectedDirectory();
    if (!selected) return;

    this.storageStore.refreshDirectory({
      deviceId: selected.deviceId,
      storageType: selected.storageType,
    });
  }

  onNavigationRequested(path: string): void {
    const selected = this.selectedDirectory();
    if (!selected) return;

    this.storageStore.navigateToDirectory({
      deviceId: selected.deviceId,
      storageType: selected.storageType,
      path: path,
    });
  }
}
