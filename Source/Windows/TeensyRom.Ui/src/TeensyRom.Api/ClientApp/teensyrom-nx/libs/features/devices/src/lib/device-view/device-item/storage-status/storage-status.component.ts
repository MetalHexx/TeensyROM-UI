import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { IconLabelComponent } from '@teensyrom-nx/ui/components';
@Component({
  selector: 'lib-storage-status',
  standalone: true,
  imports: [MatIconModule, CommonModule, MatCardModule, MatChipsModule, IconLabelComponent],
  templateUrl: './storage-status.component.html',
  styleUrl: './storage-status.component.scss',
})
export class StorageStatusComponent {
  icon = input<string>('');
  label = input<string>('');
  status = input<boolean | undefined>(undefined);
}
