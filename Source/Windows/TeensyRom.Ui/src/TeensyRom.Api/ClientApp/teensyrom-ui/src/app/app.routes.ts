import { Routes } from '@angular/router';
import { LayoutComponent } from './shell/layout/layout.component';
import { ThemeTesterComponent } from './features/theme-tester/theme-tester/theme-tester.component';

export const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: 'theme-tester',
        component: ThemeTesterComponent,
      },
    ],
  },
];
