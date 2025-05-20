import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { CommonModule } from '@angular/common';

interface StorageInfo {
  available?: boolean;
}

@Component({
  selector: 'app-storage-info',
  templateUrl: './storage-info.component.html',
  styleUrls: ['./storage-info.component.scss'],
  standalone: true,
  imports: [CommonModule, MatIconModule, MatChipsModule]
})
export class StorageInfoComponent {
  @Input() sdStorage?: StorageInfo;
  @Input() usbStorage?: StorageInfo;
} 