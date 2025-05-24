import { Component, inject } from '@angular/core';
import { NavService } from '../../../../core/services/navigation/nav.service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-nav-button',
  imports: [MatIconModule, MatButtonModule],
  templateUrl: './nav-button.component.html',
  styleUrl: './nav-button.component.scss',
})
export class NavButtonComponent {
  navService = inject(NavService);
  isNavOpen = this.navService.isNavOpen;

  toggleNav() {
    this.navService.toggleNav();
  }
}
