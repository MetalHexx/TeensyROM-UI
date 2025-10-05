import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'lib-thumbnail-image',
  imports: [CommonModule],
  templateUrl: './thumbnail-image.component.html',
  styleUrl: './thumbnail-image.component.scss'
})
export class ThumbnailImageComponent {
  imageUrl = input<string | null>(null);
  size = input<'small' | 'medium' | 'large'>('medium');
}
