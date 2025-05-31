import { Component, input, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { IconLabelComponent } from '../icon-label/icon-label.component';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'lib-status-icon-label',
  standalone: true,
  imports: [MatIconModule, CommonModule, IconLabelComponent, MatCardModule],
  templateUrl: './status-icon-label.component.html',
  styleUrl: './status-icon-label.component.scss',
})
export class StatusIconLabelComponent {
  icon = input<string>('');
  label = input<string>('');
  status = input<boolean | undefined>(undefined);
}
