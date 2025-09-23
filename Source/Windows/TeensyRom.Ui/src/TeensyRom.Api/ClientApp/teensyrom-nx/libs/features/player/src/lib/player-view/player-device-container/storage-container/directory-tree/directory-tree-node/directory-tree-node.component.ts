import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StyledIconComponent, StyledIconColor } from '@teensyrom-nx/ui/components';
import { DirectoryTreeNodeType } from '../directory-tree.component';

@Component({
  selector: 'lib-directory-tree-node',
  imports: [CommonModule, StyledIconComponent],
  templateUrl: './directory-tree-node.component.html',
  styleUrl: './directory-tree-node.component.scss',
})
export class DirectoryTreeNodeComponent {
  icon = input.required<string>();
  text = input.required<string>();
  cssClass = input<string>('');
  isSelected = input<boolean>(false);
  nodeType = input<DirectoryTreeNodeType>();

  readonly iconColor = computed<StyledIconColor>(() => {
    switch (this.nodeType()) {
      case DirectoryTreeNodeType.Device:
        return 'primary';
      case DirectoryTreeNodeType.StorageType:
        return 'highlight';
      case DirectoryTreeNodeType.Directory:
        return 'directory';
      default:
        return 'highlight';
    }
  });
}
