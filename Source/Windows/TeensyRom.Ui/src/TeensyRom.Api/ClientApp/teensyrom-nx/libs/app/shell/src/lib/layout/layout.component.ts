import { Component, computed, inject, Signal, effect } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { HeaderComponent } from '../components/header/header.component';
import { NavigationService } from '@teensyrom-nx/app/navigation';
import { NavMenuComponent } from '../components/nav-menu/nav-menu.component';
import { filter, map, mergeMap } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { DeviceStore } from '@teensyrom-nx/domain/device/state';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { BusyDialogComponent } from '../components/busy-dialog/busy-dialog.component';

@Component({
  selector: 'lib-layout',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, NavMenuComponent, MatSidenavModule],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
})
export class LayoutComponent {
  readonly deviceStore = inject(DeviceStore);
  readonly navService = inject(NavigationService);
  readonly router = inject(Router);
  readonly route = inject(ActivatedRoute);
  readonly dialog = inject(MatDialog);
  pageTitle: Signal<string>;
  showBusyDialog = computed(() => this.deviceStore.isIndexing());
  private busyDialogRef: MatDialogRef<BusyDialogComponent> | null = null;

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

    effect(() => {
      if (this.showBusyDialog() && !this.busyDialogRef) {
        this.busyDialogRef = this.dialog.open(BusyDialogComponent, {
          data: {
            message: 'This can take a few minutes.  Do not touch your commodore device.',
            title: 'Indexing Storage',
          },
          disableClose: true,
          panelClass: 'glassy-dialog',
        });
      } else if (this.busyDialogRef) {
        this.busyDialogRef.close();
        this.busyDialogRef = null;
      }
    });
  }
}
