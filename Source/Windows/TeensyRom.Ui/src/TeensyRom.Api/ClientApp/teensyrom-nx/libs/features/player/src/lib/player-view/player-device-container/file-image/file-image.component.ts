import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'lib-file-image',
  imports: [CommonModule, MatCardModule],
  templateUrl: './file-image.component.html',
  styleUrl: './file-image.component.scss',
})
export class FileImageComponent {
  title = input<string>();
  metadataSource = input<string>('Metadata Source');
}
