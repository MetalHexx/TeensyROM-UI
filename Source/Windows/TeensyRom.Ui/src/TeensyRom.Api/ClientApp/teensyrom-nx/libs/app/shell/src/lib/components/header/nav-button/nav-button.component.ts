import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NavigationService } from '@teensyrom-nx/navigation';

@Component({
  selector: 'lib-nav-button',
  imports: [MatIconModule, MatButtonModule],
  templateUrl: './nav-button.component.html',
  styleUrl: './nav-button.component.scss',
})
export class NavButtonComponent {
  navService = inject(NavigationService);
  isNavOpen = this.navService.isNavOpen;

  toggleNav() {
    this.navService.toggleNav();
  }
}
