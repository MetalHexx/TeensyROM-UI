import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DirectoryTreeComponent } from './directory-tree/directory-tree.component';
import { DirectoryFilesComponent } from './directory-files/directory-files.component';
import { SearchToolbarComponent } from './search-toolbar/search-toolbar.component';

@Component({
  selector: 'lib-storage-container',
  imports: [CommonModule, DirectoryTreeComponent, DirectoryFilesComponent, SearchToolbarComponent],
  templateUrl: './storage-container.component.html',
  styleUrl: './storage-container.component.scss',
})
export class StorageContainerComponent {}
