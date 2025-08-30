import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTreeModule } from '@angular/material/tree';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';

interface TreeNode {
  name: string;
  children?: TreeNode[];
}

@Component({
  selector: 'lib-directory-tree',
  imports: [CommonModule, MatCardModule, MatTreeModule, MatIconModule, MatButtonModule, MatChipsModule],
  templateUrl: './directory-tree.component.html',
  styleUrl: './directory-tree.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DirectoryTreeComponent {
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
                    { name: 'Folder 1.3.3.3.3',
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
