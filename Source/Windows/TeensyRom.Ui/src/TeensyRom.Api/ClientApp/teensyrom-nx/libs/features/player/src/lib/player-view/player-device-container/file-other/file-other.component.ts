import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'lib-file-other',
  imports: [CommonModule, MatCardModule],
  templateUrl: './file-other.component.html',
  styleUrl: './file-other.component.scss',
})
export class FileOtherComponent {}
