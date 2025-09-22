import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CompactCardLayoutComponent, IconButtonComponent } from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-player-toolbar',
  imports: [CommonModule, CompactCardLayoutComponent, IconButtonComponent],
  templateUrl: './player-toolbar.component.html',
  styleUrl: './player-toolbar.component.scss',
})
export class PlayerToolbarComponent {}
