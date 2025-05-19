import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DevicesService } from '../../../api-client/api/devices.service';
import { Cart } from '../../../api-client/model/cart';
import { FindDevicesResponse } from '../../../api-client/model/findDevicesResponse';

@Component({
  selector: 'app-terminal-view',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './terminal-view.component.html',
  styleUrl: './terminal-view.component.scss'
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
