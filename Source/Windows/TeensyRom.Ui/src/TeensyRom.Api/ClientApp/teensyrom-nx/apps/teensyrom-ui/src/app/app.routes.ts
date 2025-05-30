import { Route } from '@angular/router';
import { LayoutComponent } from '@teensyrom-nx/app/shell';

export const appRoutes: Route[] = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: '',
        redirectTo: 'devices',
        pathMatch: 'full',
      },
      {
        path: 'devices',
        loadComponent: () =>
          import('@teensyrom-nx/features/device').then((m) => m.DevicesComponent),
      },
    ],
  },
];
