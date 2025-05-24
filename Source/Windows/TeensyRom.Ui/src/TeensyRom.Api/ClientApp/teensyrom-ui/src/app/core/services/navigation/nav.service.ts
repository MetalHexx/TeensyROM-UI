import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { NAV_ITEMS } from './navigation.constants';
import { NavItem } from './nav-item.model';

@Injectable({
  providedIn: 'root',
})
export class NavService {
  private _router = inject(Router);
  private _isNavOpen = signal(false);
  private _navItems = signal<NavItem[]>(NAV_ITEMS);

  isNavOpen = this._isNavOpen.asReadonly();
  navItems = this._navItems.asReadonly();

  openNav() {
    this._isNavOpen.set(true);
  }

  closeNav() {
    this._isNavOpen.set(false);
  }

  toggleNav() {
    this._isNavOpen.update((isOpen) => !isOpen);
  }

  navigateTo(navItem: NavItem) {
    if (navItem.payload?.route) {
      this._router.navigate([navItem.payload.route]);
      this.closeNav();
    }
  }
}
