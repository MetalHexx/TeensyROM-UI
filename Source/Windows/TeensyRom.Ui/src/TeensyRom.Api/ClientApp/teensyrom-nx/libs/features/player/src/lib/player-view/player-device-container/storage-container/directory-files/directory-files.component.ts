import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'lib-directory-files',
  imports: [CommonModule, MatCardModule],
  templateUrl: './directory-files.component.html',
  styleUrl: './directory-files.component.scss',
})
export class DirectoryFilesComponent {}
