import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { HeaderComponent } from '../components/header/header.component';
import { NavigationService } from '@teensyrom-nx/app/navigation';
import { NavMenuComponent } from '../components/nav-menu/nav-menu.component';

@Component({
  selector: 'lib-layout',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, NavMenuComponent, MatSidenavModule],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
})
export class LayoutComponent {
  readonly navService = inject(NavigationService);
}
