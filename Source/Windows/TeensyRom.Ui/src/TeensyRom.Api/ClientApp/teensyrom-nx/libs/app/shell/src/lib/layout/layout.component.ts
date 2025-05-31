import { Component, inject, Signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { HeaderComponent } from '../components/header/header.component';
import { NavigationService } from '@teensyrom-nx/app/navigation';
import { NavMenuComponent } from '../components/nav-menu/nav-menu.component';
import { filter, map, mergeMap } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'lib-layout',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, NavMenuComponent, MatSidenavModule],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
})
export class LayoutComponent {
  readonly navService = inject(NavigationService);
  readonly router = inject(Router);
  readonly route = inject(ActivatedRoute);
  pageTitle: Signal<string>;

  constructor() {
    this.pageTitle = toSignal(
      this.router.events.pipe(
        filter((event) => event instanceof NavigationEnd),
        map(() => this.route),
        map((r) => {
          while (r.firstChild) r = r.firstChild;
          return r;
        }),
        mergeMap((r) => r.data),
        map((data) => data['title'] ?? '')
      ),
      { initialValue: '' }
    );
  }
}
