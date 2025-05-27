import { Route } from '@angular/router';

export const appRoutes: Route[] = [
    {
        path: 'devices',
        loadComponent: () => import('@teensyrom-nx/ui').then(m => m.UiComponent)
    }
];
