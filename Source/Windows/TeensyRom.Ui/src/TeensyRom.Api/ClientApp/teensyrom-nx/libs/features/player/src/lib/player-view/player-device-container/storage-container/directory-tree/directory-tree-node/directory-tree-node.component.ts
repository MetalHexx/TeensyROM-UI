import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { DirectoryTreeNodeType } from '../directory-tree.component';

@Component({
  selector: 'lib-directory-tree-node',
  imports: [CommonModule, MatIconModule],
  templateUrl: './directory-tree-node.component.html',
  styleUrl: './directory-tree-node.component.scss',
})
export class DirectoryTreeNodeComponent {
  icon = input.required<string>();
  text = input.required<string>();
  cssClass = input<string>('');
  isSelected = input<boolean>(false);
  nodeType = input<DirectoryTreeNodeType>();

  readonly iconColor = computed(() => {
    switch (this.nodeType()) {
      case DirectoryTreeNodeType.Device:
        return 'var(--color-primary-bright)';
      case DirectoryTreeNodeType.StorageType:
        return 'var(--color-highlight)';
      case DirectoryTreeNodeType.Directory:
        return 'var(--color-directory)';
      default:
        return 'var(--color-highlight)';
    }
  });
}
