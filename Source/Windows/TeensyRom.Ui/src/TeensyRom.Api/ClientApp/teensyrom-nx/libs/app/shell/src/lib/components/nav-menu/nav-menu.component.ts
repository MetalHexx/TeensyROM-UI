import { Component, computed, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MenuItemComponent } from '@teensyrom-nx/ui-components';
import { NavigationService, NavItem } from '@teensyrom-nx/app/navigation';
import { MenuItem } from '@teensyrom-nx/ui-components';

@Component({
  selector: 'lib-nav-menu',
  standalone: true,
  imports: [MatIconModule, MatListModule, MatSidenavModule, MenuItemComponent],
  templateUrl: './nav-menu.component.html',
  styleUrl: './nav-menu.component.scss',
})
export class NavMenuComponent {
  navService = inject(NavigationService);

  menuItems = computed(() =>
    this.navService.navItems().map((navItem) => ({
      name: navItem.name,
      icon: navItem.icon,
      payload: navItem,
    }))
  );
  onMenuClicked(item: MenuItem<NavItem>) {
    if (item.payload) {
      this.navService.navigateTo(item.payload);
    }
  }
}
