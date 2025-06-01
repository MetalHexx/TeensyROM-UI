import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { IconLabelComponent } from '@teensyrom-nx/ui/components';
import { MatButtonModule } from '@angular/material/button';
@Component({
  selector: 'lib-storage-status',
  imports: [
    MatIconModule,
    CommonModule,
    MatCardModule,
    MatChipsModule,
    IconLabelComponent,
    MatButtonModule,
  ],
  templateUrl: './storage-item.component.html',
  styleUrl: './storage-item.component.scss',
})
export class StorageStatusComponent {
  icon = input<string>('');
  label = input<string>('');
  status = input<boolean | undefined>(undefined);
}
