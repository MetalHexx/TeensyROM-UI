import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'lib-file-other',
  imports: [CommonModule, MatCardModule, MatChipsModule],
  templateUrl: './file-other.component.html',
  styleUrl: './file-other.component.scss',
})
export class FileOtherComponent {
  filename = input<string>('File Name');
  releaseInfo = input<string>('Release Info');
  meta1 = input<string>('Meta 1');
  meta2 = input<string>('Meta 2');
  metadataSource = input<string>('Metadata Source');
}
