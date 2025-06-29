import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'lib-directory-tree',
  imports: [CommonModule, MatCardModule],
  templateUrl: './directory-tree.component.html',
  styleUrl: './directory-tree.component.scss',
})
export class DirectoryTreeComponent {}
