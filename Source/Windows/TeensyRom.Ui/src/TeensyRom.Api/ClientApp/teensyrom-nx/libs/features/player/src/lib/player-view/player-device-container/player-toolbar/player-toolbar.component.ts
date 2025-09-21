import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CompactCardLayoutComponent } from '@teensyrom-nx/ui/components';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'lib-player-toolbar',
  imports: [CommonModule, CompactCardLayoutComponent, MatButtonModule, MatIconModule],
  templateUrl: './player-toolbar.component.html',
  styleUrl: './player-toolbar.component.scss',
})
export class PlayerToolbarComponent {}
