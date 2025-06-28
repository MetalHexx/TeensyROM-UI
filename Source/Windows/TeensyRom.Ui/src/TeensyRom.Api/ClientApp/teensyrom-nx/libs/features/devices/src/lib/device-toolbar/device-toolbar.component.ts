import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'lib-device-toolbar',
  imports: [CommonModule, MatCardModule],
  templateUrl: './device-toolbar.component.html',
  styleUrl: './device-toolbar.component.css',
})
export class DeviceToolbarComponent {}
