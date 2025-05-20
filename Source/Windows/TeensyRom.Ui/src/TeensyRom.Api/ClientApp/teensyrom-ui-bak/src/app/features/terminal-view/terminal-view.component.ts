import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DevicesService } from '../../../api-client/api/devices.service';
import { Cart } from '../../../api-client/model/cart';
import { FindDevicesResponse } from '../../../api-client/model/findDevicesResponse';
import { DeviceCardComponent } from './device-card/device-card.component';

@Component({
  selector: 'app-terminal-view',
  standalone: true,
  imports: [
    CommonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatButtonModule,
    MatIconModule,
    DeviceCardComponent
  ],
  templateUrl: './terminal-view.component.html',
  styleUrls: ['./terminal-view.component.scss']
})
export class TerminalViewComponent implements OnInit {
  devices: FindDevicesResponse = {
    availableCarts: [],
    connectedCarts: [],
    message: null
  };
  loading = false;
  error: string | null = null;

  constructor(private devicesService: DevicesService) {}

  ngOnInit(): void {
    this.loadDevices();
  }

  loadDevices(): void {
    this.loading = true;
    this.error = null;
    
    this.devicesService.devicesGet().subscribe({
      next: (response) => {
        this.devices = response;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load devices. Please try again.';
        this.loading = false;
        console.error('Error loading devices:', err);
      }
    });
  }
}
