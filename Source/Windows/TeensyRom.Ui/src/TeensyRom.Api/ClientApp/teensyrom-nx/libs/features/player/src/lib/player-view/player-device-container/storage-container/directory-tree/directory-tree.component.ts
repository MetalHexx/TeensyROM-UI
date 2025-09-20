import {
  ChangeDetectionStrategy,
  Component,
  input,
  inject,
  computed,
  signal,
  effect,
  viewChild,
  AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTreeModule, MatTree } from '@angular/material/tree';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { StorageStore, StorageDirectoryState } from '@teensyrom-nx/domain/storage/state';
import { StorageType, StorageDirectory } from '@teensyrom-nx/domain/storage/services';
import { LogType, logInfo } from '@teensyrom-nx/utils';

export enum DirectoryTreeNodeType {
  Device = 'device',
  StorageType = 'storage',
  Directory = 'directory',
  Placeholder = 'placeholder',
}

export interface DirectoryTreeNode {
  id: string;
  name: string;
  type: DirectoryTreeNodeType;
  icon: string;
  deviceId?: string;
  storageType?: StorageType;
  path?: string;
  isLoading?: boolean;
  error?: string | null;
  children?: DirectoryTreeNode[];
}

interface DirectoryCacheEntry {
  directory: StorageDirectory;
}

@Component({
  selector: 'lib-directory-tree',
  imports: [
    CommonModule,
    MatCardModule,
    MatTreeModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
  ],
  templateUrl: './directory-tree.component.html',
  styleUrl: './directory-tree.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DirectoryTreeComponent implements AfterViewInit {
  deviceId = input.required<string>();

  private readonly storageStore = inject(StorageStore);
  private readonly tree = viewChild<MatTree<DirectoryTreeNode>>('tree');

  readonly directories = computed(() => this.storageStore.getDeviceDirectories(this.deviceId())());

  private readonly directoryCache = signal<Map<string, DirectoryCacheEntry>>(new Map());

  childrenAccessor = (node: DirectoryTreeNode) => node.children ?? [];
  hasChild = (_: number, node: DirectoryTreeNode) => !!node.children && node.children.length > 0;
  isPlaceholder = (_: number, node: DirectoryTreeNode) =>
    node.type === DirectoryTreeNodeType.Placeholder;
  trackByFn = (_: number, node: DirectoryTreeNode) => node.id;
  expansionKeyFn = (node: DirectoryTreeNode) => node.id;

  ngAfterViewInit() {
    this.autoExpandDirectoryNode();
  }

  private autoExpandDirectoryNode() {
    const treeComponent = this.tree();
    if (treeComponent) {
      setTimeout(() => {
        const deviceNodes = this.directoryTree().filter(
          (node) => node.type === DirectoryTreeNodeType.Device
        );
        deviceNodes.forEach((deviceNode) => {
          treeComponent.expand(deviceNode);
        });
      });
    }
  }

  private createCacheKey(deviceId: string, storageType: StorageType, path: string): string {
    return `${deviceId}-${storageType}-${path}`;
  }

  private getCachedDirectory(
    deviceId: string,
    storageType: StorageType,
    path: string
  ): DirectoryCacheEntry | undefined {
    const key = this.createCacheKey(deviceId, storageType, path);
    return this.directoryCache().get(key);
  }

  private setCachedDirectory(
    deviceId: string,
    storageType: StorageType,
    path: string,
    directory: StorageDirectory
  ): void {
    const key = this.createCacheKey(deviceId, storageType, path);
    const entry: DirectoryCacheEntry = {
      directory,
    };

    this.directoryCache.update((cache) => {
      const newCache = new Map(cache);
      newCache.set(key, entry);
      return newCache;
    });
  }

  // Use effect to sync store data to cache (cannot write to signals in computed)
  private readonly storeCacheSync = effect(() => {
    const deviceDirectories = this.directories();
    this.syncStoreDataToCache(deviceDirectories);
  });

  readonly directoryTree = computed(() => {
    const deviceId = this.deviceId();

    // Access cache signal to create dependency for reactivity
    this.directoryCache();

    return this.buildTreeNodes(deviceId);
  });

  private syncStoreDataToCache(deviceDirectories: StorageDirectoryState[]): void {
    deviceDirectories.forEach((entry) => {
      if (entry.directory) {
        this.setCachedDirectory(entry.deviceId, entry.storageType, entry.currentPath, {
          directories: entry.directory.directories,
          files: [], // Files handled separately in DirectoryFiles component
          path: entry.currentPath,
        });
      }
    });
  }

  private buildTreeNodes(deviceId: string): DirectoryTreeNode[] {
    const deviceNode: DirectoryTreeNode = {
      id: `device-${deviceId}`,
      name: `Device ${deviceId}`,
      type: DirectoryTreeNodeType.Device,
      icon: 'smartphone',
      deviceId,
      children: this.buildStorageTypeNodes(deviceId),
    };

    return [deviceNode];
  }

  private buildStorageTypeNodes(deviceId: string): DirectoryTreeNode[] {
    const storageNodes: DirectoryTreeNode[] = [];

    Object.values(StorageType).forEach((storageType) => {
      const storageNodeId = `${deviceId}-${storageType}`;
      const directoryChildren = this.buildDirectoryNodes(deviceId, storageType, '/');

      const storageNode: DirectoryTreeNode = {
        id: storageNodeId,
        name: `${storageType} Storage`,
        type: DirectoryTreeNodeType.StorageType,
        icon: this.getStorageTypeIcon(storageType),
        deviceId,
        storageType,
        path: '/',
        children:
          directoryChildren.length > 0
            ? directoryChildren
            : this.createPlaceholderChildren(storageNodeId),
      };

      storageNodes.push(storageNode);
    });

    return storageNodes;
  }

  private buildDirectoryNodes(
    deviceId: string,
    storageType: StorageType,
    parentPath: string
  ): DirectoryTreeNode[] {
    const cachedDirectory = this.getCachedDirectory(deviceId, storageType, parentPath);

    if (!cachedDirectory) {
      return [];
    }

    return cachedDirectory.directory.directories.map((dir) => {
      const nodeId = `${deviceId}-${storageType}-${dir.path}`;
      const childDirectories = this.buildDirectoryNodes(deviceId, storageType, dir.path);

      // Check if we have cached data for this child directory
      const childCachedData = this.getCachedDirectory(deviceId, storageType, dir.path);

      return {
        id: nodeId,
        name: dir.name,
        type: DirectoryTreeNodeType.Directory,
        icon: 'folder',
        deviceId,
        storageType,
        path: dir.path,
        children: this.determineChildren(childDirectories, childCachedData, nodeId),
      };
    });
  }

  private determineChildren(
    childDirectories: DirectoryTreeNode[],
    childCachedData: DirectoryCacheEntry | undefined,
    nodeId: string
  ): DirectoryTreeNode[] {
    // If we have child directories, return them
    if (childDirectories.length > 0) {
      return childDirectories;
    }

    // If we have cached data for this directory, it means it was loaded
    if (childCachedData) {
      // If it has no subdirectories, don't show placeholder (it's a leaf)
      return [];
    }

    // No cached data means not loaded yet, show placeholder
    return this.createPlaceholderChildren(nodeId);
  }

  private createPlaceholderChildren(parentNodeId: string): DirectoryTreeNode[] {
    return [
      {
        id: `${parentNodeId}-placeholder`,
        name: 'Loading...',
        type: DirectoryTreeNodeType.Placeholder,
        icon: 'hourglass_empty',
      },
    ];
  }

  private getStorageTypeIcon(storageType: StorageType): string {
    switch (storageType) {
      case StorageType.Sd:
        return 'sd_storage';
      case StorageType.Usb:
        return 'usb';
      default:
        return 'storage';
    }
  }

  getNodeClasses(node: DirectoryTreeNode): string {
    const classes: string[] = [];

    if (node.type === DirectoryTreeNodeType.Device) {
      classes.push('device-node');
    } else if (node.type === DirectoryTreeNodeType.StorageType) {
      classes.push('storage-node');
    } else if (node.type === DirectoryTreeNodeType.Directory) {
      classes.push('directory-node');
    } else if (node.type === DirectoryTreeNodeType.Placeholder) {
      classes.push('placeholder-node');
    }

    if (node.isLoading) {
      classes.push('loading');
    }

    if (node.error) {
      classes.push('error');
    }

    return classes.join(' ');
  }

  onDirectoryClick(node: DirectoryTreeNode) {
    logInfo(LogType.Select, `Directory selected: ${node.name}`, node);

    // Only trigger navigation for directories and storage types
    if (
      (node.type === DirectoryTreeNodeType.Directory ||
        node.type === DirectoryTreeNodeType.StorageType) &&
      node.deviceId &&
      node.storageType &&
      node.path
    ) {
      this.storageStore.navigateToDirectory({
        deviceId: node.deviceId,
        storageType: node.storageType,
        path: node.path,
      });
    }
  }

  onToggleClick(node: DirectoryTreeNode) {
    // Use setTimeout to let the Material Tree update its expansion state first
    setTimeout(() => {
      const treeComponent = this.tree();
      if (treeComponent) {
        const isExpanded = treeComponent.isExpanded(node);

        // Only proceed if the node is now expanded
        if (isExpanded) {
          // Check if the expanded node has placeholder children
          const hasPlaceholderChildren =
            node.children &&
            node.children.length === 1 &&
            node.children[0].type === DirectoryTreeNodeType.Placeholder;

          if (hasPlaceholderChildren) {
            // Trigger data loading for this node
            if (
              (node.type === DirectoryTreeNodeType.Directory ||
                node.type === DirectoryTreeNodeType.StorageType) &&
              node.deviceId &&
              node.storageType &&
              node.path
            ) {
              this.storageStore.navigateToDirectory({
                deviceId: node.deviceId,
                storageType: node.storageType,
                path: node.path,
              });
            }
          }
        }
      }
    }, 50);
  }
}
