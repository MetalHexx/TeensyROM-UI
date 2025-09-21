import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardLayoutComponent } from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-file-image',
  imports: [CommonModule, CardLayoutComponent],
  templateUrl: './file-image.component.html',
  styleUrl: './file-image.component.scss',
})
export class FileImageComponent {
  creatorName = input<string>();
  metadataSource = input<string>('Metadata Source');
}
