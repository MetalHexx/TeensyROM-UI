import { Component, computed, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { NavService } from '../../../core/services/navigation/nav.service';
import { MenuItem } from '../../../libs/components/menu-item/menu-item.model';
import { MenuItemComponent } from '../../../libs/components/menu-item/menu-item.component';
import { NavItem } from '@app/core/services/navigation/nav-item.model';

@Component({
  selector: 'app-nav-menu',
  standalone: true,
  imports: [MatIconModule, MatListModule, MatSidenavModule, MenuItemComponent],
  templateUrl: './nav-menu.component.html',
  styleUrl: './nav-menu.component.scss',
})
export class NavMenuComponent {
  navService = inject(NavService);

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
