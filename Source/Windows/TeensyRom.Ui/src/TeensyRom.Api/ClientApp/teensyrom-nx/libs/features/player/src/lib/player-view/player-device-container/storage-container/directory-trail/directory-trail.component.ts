import { Component, input, inject, computed } from '@angular/core';
import { StorageStore, PLAYER_CONTEXT, IPlayerContext } from '@teensyrom-nx/application';
import { ScalingCompactCardComponent } from '@teensyrom-nx/ui/components';
import { DirectoryNavigateComponent } from './directory-navigate/directory-navigate.component';
import { DirectoryBreadcrumbComponent } from './directory-breadcrumb/directory-breadcrumb.component';

@Component({
  selector: 'lib-directory-trail',
  standalone: true,
  imports: [ScalingCompactCardComponent, DirectoryNavigateComponent, DirectoryBreadcrumbComponent],
  templateUrl: './directory-trail.component.html',
  styleUrl: './directory-trail.component.scss',
})
export class DirectoryTrailComponent {
  // Inputs
  deviceId = input.required<string>();

  // Store injection
  private readonly storageStore = inject(StorageStore);
  private readonly playerContext: IPlayerContext = inject(PLAYER_CONTEXT);

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

  canNavigateBack = computed(() => {
    const deviceId = this.deviceId();
    const history = this.storageStore.navigationHistory()[deviceId];
    return !!(history && history.currentIndex > 0);
  });

  canNavigateForward = computed(() => {
    const deviceId = this.deviceId();
    const history = this.storageStore.navigationHistory()[deviceId];
    return !!(history && history.currentIndex < history.history.length - 1);
  });

  isLoading = computed(() => {
    const state = this.selectedDirectoryState();
    return state?.isLoading ?? false;
  });

  historyViewVisible = computed(() => {
    const deviceId = this.deviceId();
    return this.playerContext.isHistoryViewVisible(deviceId)();
  });

  onBackClick(): void {
    const deviceId = this.deviceId();
    if (this.canNavigateBack()) {
      this.storageStore.navigateDirectoryBackward({ deviceId });
    }
  }

  onForwardClick(): void {
    const deviceId = this.deviceId();
    if (this.canNavigateForward()) {
      this.storageStore.navigateDirectoryForward({ deviceId });
    }
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

  onHistoryToggleClick(): void {
    const deviceId = this.deviceId();
    this.playerContext.toggleHistoryView(deviceId);
  }
}
