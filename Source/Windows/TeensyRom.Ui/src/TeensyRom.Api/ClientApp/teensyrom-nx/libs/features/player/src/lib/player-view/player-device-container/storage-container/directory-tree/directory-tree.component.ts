import { ChangeDetectionStrategy, Component, input, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTreeModule } from '@angular/material/tree';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { StorageStore } from '@teensyrom-nx/domain/storage/state';

interface TreeNode {
  name: string;
  children?: TreeNode[];
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
export class DirectoryTreeComponent {
  // Signals-based device context
  deviceId = input.required<string>();

  private readonly storageStore = inject(StorageStore);

  // Directories-only JSON projection for this device across storage types
  readonly directories = computed(() =>
    this.storageStore['getDeviceDirectories'](this.deviceId())()
  );

  childrenAccessor = (node: TreeNode) => node.children ?? [];
  hasChild = (_: number, node: TreeNode) => !!node.children && node.children.length > 0;

  directoryTree: TreeNode[] = [
    {
      name: 'Folder 1',
      children: [
        { name: 'Folder 1.1' },
        { name: 'Folder 1.2' },
        {
          name: 'Folder 1.3',
          children: [
            { name: 'Folder 1.3.1' },
            { name: 'Folder 1.3.2' },
            {
              name: 'Folder 1.3.3',
              children: [
                { name: 'Folder 1.3.3.1' },
                { name: 'Folder 1.3.3.2' },
                {
                  name: 'Folder 1.3.3.3',
                  children: [
                    { name: 'Folder 1.3.3.3.1' },
                    { name: 'Folder 1.3.3.3.2' },
                    {
                      name: 'Folder 1.3.3.3.3',
                      children: [
                        { name: 'Folder 1.3.3.3.3.1' },
                        { name: 'Folder 1.3.3.3.3.2' },
                        { name: 'Folder 1.3.3.3.3.3' },
                      ],
                    },
                  ],
                },
              ],
            },
          ],
        },
      ],
    },
  ];

  onDirectoryClick(node: TreeNode) {
    console.log(node);
  }
}
