import { Route } from '@angular/router';

export const appRoutes: Route[] = [
  {
    path: '',
    redirectTo: 'devices',
    pathMatch: 'full',
  },
  {
    path: 'devices',
    loadComponent: () => import('@teensyrom-nx/device-features').then((m) => m.DevicesComponent),
  },
];
