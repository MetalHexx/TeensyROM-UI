import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { IconLabelComponent } from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-device-toolbar',
  imports: [CommonModule, MatCardModule, MatExpansionModule, MatIconModule, IconLabelComponent],
  templateUrl: './device-toolbar.component.html',
  styleUrl: './device-toolbar.component.css',
})
export class DeviceToolbarComponent {}
